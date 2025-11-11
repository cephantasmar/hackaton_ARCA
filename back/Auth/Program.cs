using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Cargar .env
DotNetEnv.Env.Load();

// 🔹 Configuración Supabase
var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") 
    ?? throw new ArgumentNullException("SUPABASE_URL no configurado");
var supabaseAnonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") 
    ?? throw new ArgumentNullException("SUPABASE_ANON_KEY no configurado");
var supabaseJwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") ?? supabaseAnonKey;
var supabaseServiceRoleKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY") 
    ?? throw new ArgumentNullException("SUPABASE_SERVICE_ROLE_KEY no configurado");

// 🔹 JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"{supabaseUrl}/auth/v1",
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseJwtSecret!)),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// 🔹 CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://frontend:80")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ═══════════════════════════════════════════════════════════════
// 🔹 FUNCIONES HELPER - UNA SOLA TABLA
// ═══════════════════════════════════════════════════════════════

// 🔹 HttpClient configurado una sola vez
HttpClient CreateSupabaseClient(string apiKey)
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("apikey", apiKey);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    return client;
}

// 🔹 Split nombre simplificado
(string nombre, string apellido) SplitFullName(string fullName)
{
    if (string.IsNullOrWhiteSpace(fullName))
        return ("Usuario", "");

    var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    
    return parts.Length switch
    {
        0 => ("Usuario", ""),
        1 => (parts[0], ""),
        2 => (parts[0], parts[1]),
        3 => (parts[0], parts[2]),
        4 => ($"{parts[0]} {parts[1]}", $"{parts[2]} {parts[3]}"),
        _ => ($"{parts[0]} {parts[1]}", parts[2])
    };
}

// 🔹 Verificar si usuario existe en tabla única
async Task<bool> CheckUserExists(string email)
{
    try
    {
        using var httpClient = CreateSupabaseClient(supabaseServiceRoleKey);
        var url = $"{supabaseUrl}/rest/v1/usuarios?email=eq.{Uri.EscapeDataString(email)}&select=id";
        
        var response = await httpClient.GetAsync(url);
        return response.IsSuccessStatusCode && 
               (await response.Content.ReadAsStringAsync()).Contains("id");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Exception verificando usuario: {ex.Message}");
        return false;
    }
}

// 🔹 Crear usuario en tabla única
async Task<bool> CreateUserViaSupabaseAPI(string email, string fullName)
{
    try
    {
        using var httpClient = CreateSupabaseClient(supabaseServiceRoleKey);
        httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

        var (nombre, apellido) = SplitFullName(fullName);
        
        // Asignar rol basado en el dominio del email
        var rol = email.EndsWith("@ucb.edu.bo") ? "Admin" : "Empleado";
        
        var userData = new { 
            nombre, 
            apellido, 
            email, 
            rol,
            created_at = DateTime.UtcNow 
        };

        var jsonContent = JsonSerializer.Serialize(userData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var response = await httpClient.PostAsync($"{supabaseUrl}/rest/v1/usuarios", content);
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Exception creando usuario: {ex.Message}");
        return false;
    }
}

// 🔹 Obtener usuario por email desde tabla única
async Task<JsonElement?> GetUserByEmail(string email)
{
    try
    {
        using var httpClient = CreateSupabaseClient(supabaseServiceRoleKey);
        var url = $"{supabaseUrl}/rest/v1/usuarios?email=eq.{Uri.EscapeDataString(email)}&select=*";
        
        var response = await httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<JsonElement[]>(content);
            return users?.Length > 0 ? users[0] : null;
        }
        
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Exception obteniendo usuario: {ex.Message}");
        return null;
    }
}

// ═══════════════════════════════════════════════════════════════
// 🔹 ENDPOINTS ACTUALIZADOS - UNA SOLA TABLA
// ═══════════════════════════════════════════════════════════════

app.MapPost("/api/auth/sync-user", async (HttpContext context) =>
{
    try
    {
        var email = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        var fullName = context.User.FindFirst("name")?.Value ?? 
                      context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? 
                      "Usuario";

        if (string.IsNullOrEmpty(email))
            return Results.BadRequest(new { error = "Email no encontrado en el token." });

        // Verificar que el email esté verificado en Supabase
        var emailVerified = context.User.FindFirst("email_verified")?.Value;
        if (emailVerified != "true")
        {
            // Para Google OAuth, verificar otra claim
            var hasEmailVerified = context.User.Claims.Any(c => c.Type == "email_verified" && c.Value == "true");
            if (!hasEmailVerified)
            {
                return Results.BadRequest(new { error = "Email no verificado. Por favor, verifica tu email antes de continuar." });
            }
        }

        if (!await CheckUserExists(email))
        {
            var success = await CreateUserViaSupabaseAPI(email, fullName);
            return success ? 
                Results.Ok(new { success = true, message = "Usuario creado", email, isNewUser = true }) :
                Results.Problem("Error al crear usuario.");
        }

        return Results.Ok(new { success = true, message = "Usuario ya existe", email, isNewUser = false });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"❌ Error en sync-user: {ex.Message}");
        return Results.Problem("Error interno del servidor.");
    }
})
.RequireAuthorization()
.WithName("SyncUser")
.WithOpenApi();

app.MapGet("/api/auth/user-profile", async (HttpContext context) =>
{
    try
    {
        var email = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        
        if (string.IsNullOrEmpty(email))
            return Results.Unauthorized();

        var user = await GetUserByEmail(email);
        if (user == null)
            return Results.NotFound(new { error = "Usuario no encontrado." });

        // Manejar la propiedad remuneracion de forma segura
        decimal? remuneracion = null;
        if (user.Value.TryGetProperty("remuneracion", out var remProp) && remProp.ValueKind == JsonValueKind.Number)
        {
            remuneracion = remProp.GetDecimal();
        }

        var profile = new
        {
            id = user.Value.GetProperty("id").GetString(),
            nombre = user.Value.GetProperty("nombre").GetString() ?? "Usuario",
            apellido = user.Value.GetProperty("apellido").GetString() ?? "",
            email = user.Value.GetProperty("email").GetString() ?? email,
            rol = user.Value.GetProperty("rol").GetString() ?? "Empleado",
            remuneracion = remuneracion,
            cargo = user.Value.TryGetProperty("cargo", out var cargo) ? cargo.GetString() : null,
            created_at = user.Value.TryGetProperty("created_at", out var createdAt) ? createdAt.GetString() : null
        };
        
        return Results.Ok(profile);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"❌ Error obteniendo perfil: {ex.Message}");
        return Results.Problem("Error interno del servidor.");
    }
})
.RequireAuthorization()
.WithName("GetUserProfile")
.WithOpenApi();

app.MapGet("/api/usuarios", async () =>
{
    try
    {
        using var httpClient = CreateSupabaseClient(supabaseServiceRoleKey);
        var response = await httpClient.GetAsync($"{supabaseUrl}/rest/v1/usuarios?select=*&order=created_at.desc");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<JsonElement[]>(content) ?? Array.Empty<JsonElement>();
            
            return Results.Ok(new
            {
                total = usuarios.Length,
                usuarios
            });
        }
        else
        {
            return Results.Problem("Error al obtener usuarios.");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"❌ Error obteniendo usuarios: {ex.Message}");
        return Results.Problem("Error al obtener usuarios.");
    }
})
.RequireAuthorization()
.WithName("GetAllUsuarios")
.WithOpenApi();

// Endpoints básicos
app.MapGet("/", () => "API con tabla única de usuarios 🚀");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

Console.WriteLine("🚀 API con tabla única de usuarios iniciada correctamente");
app.Run();