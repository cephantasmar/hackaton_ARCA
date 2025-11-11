using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuración
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient para Supabase
builder.Services.AddHttpClient("Supabase", client =>
{
    client.BaseAddress = new Uri("https://nnqbpvbcdwcodnradhye.supabase.co/rest/v1/");
    client.DefaultRequestHeaders.Add("apikey", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5ucWJwdmJjZHdjb2RucmFkaHllIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTg5MzYzMTcsImV4cCI6MjA3NDUxMjMxN30.ZYbcRG9D2J0SlhcT9XTzGX5AAW5wuTXPnzmkbC_pGPU");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5ucWJwdmJjZHdjb2RucmFkaHllIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTg5MzYzMTcsImV4cCI6MjA3NDUxMjMxN30.ZYbcRG9D2J0SlhcT9XTzGX5AAW5wuTXPnzmkbC_pGPU");
});

// CORS específico para tus puertos
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://frontend:80")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();

// Helper para crear cliente HTTP
HttpClient CreateSupabaseClient()
{
    var client = new HttpClient();
    client.BaseAddress = new Uri("https://nnqbpvbcdwcodnradhye.supabase.co/rest/v1/");
    client.DefaultRequestHeaders.Add("apikey", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5ucWJwdmJjZHdjb2RucmFkaHllIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTg5MzYzMTcsImV4cCI6MjA3NDUxMjMxN30.ZYbcRG9D2J0SlhcT9XTzGX5AAW5wuTXPnzmkbC_pGPU");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5ucWJwdmJjZHdjb2RucmFkaHllIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTg5MzYzMTcsImV4cCI6MjA3NDUxMjMxN30.ZYbcRG9D2J0SlhcT9XTzGX5AAW5wuTXPnzmkbC_pGPU");
    return client;
}

// Endpoint para obtener todos los usuarios
app.MapGet("/api/usuarios", async () =>
{
    try
    {
        Console.WriteLine("🔍 Obteniendo todos los usuarios");
        
        using var client = CreateSupabaseClient();
        var response = await client.GetAsync("usuarios?select=*&order=nombre.asc");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<JsonElement[]>(content) ?? Array.Empty<JsonElement>();
            
            Console.WriteLine($"✅ {usuarios.Length} usuarios encontrados");
            return Results.Ok(new { 
                usuarios,
                total = usuarios.Length
            });
        }
        else
        {
            Console.WriteLine($"❌ Error HTTP: {response.StatusCode}");
            return Results.Problem("Error al obtener usuarios de Supabase");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error obteniendo usuarios: {ex.Message}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Endpoint para obtener usuario por ID
app.MapGet("/api/usuarios/{id}", async (string id) =>
{
    try
    {
        Console.WriteLine($"🔍 Obteniendo usuario: {id}");
        
        using var client = CreateSupabaseClient();
        var response = await client.GetAsync($"usuarios?id=eq.{id}&select=*");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<JsonElement[]>(content);
            
            if (usuarios?.Length > 0)
            {
                return Results.Ok(usuarios[0]);
            }
            else
            {
                return Results.NotFound("Usuario no encontrado");
            }
        }
        
        return Results.NotFound("Usuario no encontrado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error obteniendo usuario: {ex.Message}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Endpoint para actualizar rol
app.MapPut("/api/usuarios/{id}/rol", async (string id, [FromBody] ActualizarRolRequest request) =>
{
    try
    {
        Console.WriteLine($"🔧 Actualizando rol para usuario {id}");
        
        // Validar que el rol sea uno de los permitidos
        var rolesPermitidos = new[] { "Admin", "Empleado" };
        if (!rolesPermitidos.Contains(request.Rol))
        {
            return Results.BadRequest($"Rol no válido. Los roles permitidos son: {string.Join(", ", rolesPermitidos)}");
        }

        using var client = CreateSupabaseClient();
        client.DefaultRequestHeaders.Add("Prefer", "return=representation");
        
        var updateData = new { 
            rol = request.Rol,
            updated_at = DateTime.UtcNow
        };

        var jsonContent = JsonSerializer.Serialize(updateData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var response = await client.PatchAsync($"usuarios?id=eq.{id}", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"✅ Rol actualizado: Usuario {id} ahora es {request.Rol}");
            return Results.Ok(new { 
                mensaje = "Rol actualizado correctamente", 
                usuarioId = id,
                nuevoRol = request.Rol 
            });
        }

        return Results.NotFound("Usuario no encontrado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error actualizando rol: {ex.Message}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Endpoint para obtener los roles disponibles
app.MapGet("/api/roles", () =>
{
    var roles = new[] { "Admin", "Empleado" };
    return Results.Ok(roles);
});

// Endpoint para crear usuario
app.MapPost("/api/usuarios", async ([FromBody] CrearUsuarioRequest request) =>
{
    try
    {
        Console.WriteLine($"👤 Creando nuevo usuario: {request.Email}");

        // Validar campos requeridos
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.Apellido))
        {
            return Results.BadRequest("Nombre, apellido y email son requeridos");
        }

        // Verificar si el email ya existe
        using var checkClient = CreateSupabaseClient();
        var checkResponse = await checkClient.GetAsync($"usuarios?email=eq.{Uri.EscapeDataString(request.Email)}&select=id");
        
        if (checkResponse.IsSuccessStatusCode)
        {
            var checkContent = await checkResponse.Content.ReadAsStringAsync();
            var existingUsers = JsonSerializer.Deserialize<JsonElement[]>(checkContent);
            if (existingUsers?.Length > 0)
            {
                return Results.BadRequest("El email ya está registrado");
            }
        }

        // Crear nuevo usuario
        using var client = CreateSupabaseClient();
        client.DefaultRequestHeaders.Add("Prefer", "return=representation");
        
        var nuevoUsuario = new
        {
            nombre = request.Nombre,
            apellido = request.Apellido,
            email = request.Email,
            rol = request.Rol ?? "Empleado",
            remuneracion = request.Remuneracion,
            cargo = request.Cargo,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        var jsonContent = JsonSerializer.Serialize(nuevoUsuario);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("usuarios", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var usuariosCreados = JsonSerializer.Deserialize<JsonElement[]>(responseContent);
            
            if (usuariosCreados?.Length > 0)
            {
                var usuarioCreado = usuariosCreados[0];
                return Results.Ok(new
                {
                    mensaje = "Usuario creado correctamente",
                    usuario = new
                    {
                        Id = usuarioCreado.GetProperty("id").GetString(),
                        Nombre = usuarioCreado.GetProperty("nombre").GetString(),
                        Apellido = usuarioCreado.GetProperty("apellido").GetString(),
                        Email = usuarioCreado.GetProperty("email").GetString(),
                        Rol = usuarioCreado.GetProperty("rol").GetString(),
                        Cargo = usuarioCreado.TryGetProperty("cargo", out var cargo) ? cargo.GetString() : null,
                        Remuneracion = usuarioCreado.TryGetProperty("remuneracion", out var rem) && rem.ValueKind == JsonValueKind.Number ? rem.GetDecimal() : (decimal?)null
                    }
                });
            }
        }

        return Results.Problem("Error al crear el usuario");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error creando usuario: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Endpoint para eliminar usuario (borrado físico)
app.MapDelete("/api/usuarios/{id}", async (string id) =>
{
    try
    {
        Console.WriteLine($"🗑️ Eliminando usuario: {id}");

        using var client = CreateSupabaseClient();
        client.DefaultRequestHeaders.Add("Prefer", "return=representation");
        
        var response = await client.DeleteAsync($"usuarios?id=eq.{id}");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"✅ Usuario {id} eliminado correctamente");
            return Results.Ok(new { 
                mensaje = "Usuario eliminado correctamente", 
                usuarioId = id 
            });
        }

        return Results.NotFound("Usuario no encontrado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error eliminando usuario: {ex.Message}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Endpoint para actualizar información completa del usuario
app.MapPut("/api/usuarios/{id}", async (string id, [FromBody] ActualizarUsuarioRequest request) =>
{
    try
    {
        Console.WriteLine($"✏️ Actualizando usuario: {id}");

        using var client = CreateSupabaseClient();
        client.DefaultRequestHeaders.Add("Prefer", "return=representation");
        
        var updateData = new
        {
            nombre = request.Nombre,
            apellido = request.Apellido,
            rol = request.Rol,
            cargo = request.Cargo,
            remuneracion = request.Remuneracion,
            updated_at = DateTime.UtcNow
        };

        var jsonContent = JsonSerializer.Serialize(updateData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var response = await client.PatchAsync($"usuarios?id=eq.{id}", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"✅ Usuario {id} actualizado correctamente");
            return Results.Ok(new { 
                mensaje = "Usuario actualizado correctamente", 
                usuarioId = id 
            });
        }

        return Results.NotFound("Usuario no encontrado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error actualizando usuario: {ex.Message}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Endpoint para filtrar usuarios
app.MapGet("/api/usuarios/filtrar", async ([FromQuery] string? rol, [FromQuery] string? cargo) =>
{
    try
    {
        Console.WriteLine($"🔍 Filtrando usuarios - Rol: {rol ?? "Todos"}, Cargo: {cargo ?? "Todos"}");
        
        var query = "usuarios?select=*";
        
        if (!string.IsNullOrEmpty(rol))
        {
            query += $"&rol=eq.{Uri.EscapeDataString(rol)}";
        }
            
        if (!string.IsNullOrEmpty(cargo))
        {
            query += $"&cargo=eq.{Uri.EscapeDataString(cargo)}";
        }

        query += "&order=nombre.asc";

        using var client = CreateSupabaseClient();
        var response = await client.GetAsync(query);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<JsonElement[]>(content) ?? Array.Empty<JsonElement>();

            Console.WriteLine($"✅ {usuarios.Length} usuarios encontrados con los filtros aplicados");
            return Results.Ok(new { 
                usuarios,
                total = usuarios.Length,
                filtroRol = rol,
                filtroCargo = cargo
            });
        }

        return Results.Problem("Error al filtrar usuarios");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error filtrando usuarios: {ex.Message}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Endpoint para buscar usuarios
app.MapGet("/api/usuarios/buscar", async ([FromQuery] string? search) =>
{
    try
    {
        Console.WriteLine($"🔍 Buscando usuarios: {search ?? "Todos"}");
        
        using var client = CreateSupabaseClient();
        var response = await client.GetAsync("usuarios?select=*&order=nombre.asc");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<JsonElement[]>(content) ?? Array.Empty<JsonElement>();

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                usuarios = usuarios.Where(u => 
                    (u.GetProperty("nombre").GetString()?.ToLower().Contains(searchLower) == true) ||
                    (u.GetProperty("apellido").GetString()?.ToLower().Contains(searchLower) == true) ||
                    (u.GetProperty("email").GetString()?.ToLower().Contains(searchLower) == true) ||
                    (u.TryGetProperty("cargo", out var cargo) && cargo.GetString()?.ToLower().Contains(searchLower) == true)
                ).ToArray();
            }

            Console.WriteLine($"✅ {usuarios.Length} usuarios encontrados en la búsqueda");
            return Results.Ok(new { 
                usuarios,
                total = usuarios.Length,
                busqueda = search
            });
        }

        return Results.Problem("Error al buscar usuarios");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error buscando usuarios: {ex.Message}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Endpoints de health check
app.MapGet("/health", () => 
{
    Console.WriteLine("🔍 Health check solicitado");
    return Results.Ok(new { status = "Healthy", service = "Roles", timestamp = DateTime.Now });
});

app.MapGet("/test", () => 
{
    Console.WriteLine("✅ Test endpoint funcionando");
    return Results.Ok(new { message = "Backend funcionando correctamente", status = "OK" });
});

app.Run();

// Modelos para requests
public class CrearUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Rol { get; set; }
    public decimal? Remuneracion { get; set; }
    public string? Cargo { get; set; }
}

public class ActualizarRolRequest
{
    public string Rol { get; set; } = string.Empty;
}

public class ActualizarUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public decimal? Remuneracion { get; set; }
}