<template>
  <footer class="footer">
    <div class="inner">
      <div class="left">
        <div class="brand">EMPRESA DE SERVICIOS AUXILIARES FINANCIEROS ARCA LTDA.</div>
        <div class="tag">MVP Gestión Humana — prueba rápida y modular</div>
      </div>

      <nav class="links" aria-label="Enlaces rápidos">
        <router-link to="/personal">Personal</router-link>
        <router-link to="/vacaciones">Vacaciones</router-link>
        <router-link to="/contratos">Contratos</router-link>
        <router-link to="/boletas">Boletas</router-link>
      </nav>

      <div class="right">
        <small>© {{ year }} ARCA LTDA.</small>
      </div>
    </div>
  </footer>
</template>

<script setup>
const year = new Date().getFullYear()
</script>

<style scoped>
.footer {
  background: #0b1220;
  color: #cbd5e1;
  padding: 12px 0;
  border-top: 1px solid rgba(255,255,255,0.03);
}
.inner {
  max-width: 1100px;
  margin: 0 auto;
  padding: 0 16px;
  display: flex;
  gap: 12px;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
}
.left { display:flex; flex-direction:column; }
.brand { font-weight:700; color:#fff; font-size:0.95rem; }
.tag { font-size:0.85rem; color:#9ca3af; margin-top:4px; }

.links { display:flex; gap:10px; align-items:center; }
.links a {
  color: #93c5fd;
  text-decoration: none;
  font-size: 0.9rem;
  padding: 6px 8px;
  border-radius: 6px;
  transition: background .15s, color .15s;
}
.links a:hover { background: rgba(255,255,255,0.02); color: #fff; }

.right small { color: #94a3b8; }

@media (max-width: 600px) {
  .inner { flex-direction: column; align-items: flex-start; }
  .links { width: 100%; justify-content: flex-start; margin: 8px 0; flex-wrap:wrap; gap:8px; }
}
</style>