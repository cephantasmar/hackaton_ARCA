import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    open: true,
    proxy: {
      '/api/vacations': {
        target: 'http://localhost:5010',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/vacations/, '/api/vacations')
      }
    }
  },
  build: {
    outDir: 'dist',
  },
});