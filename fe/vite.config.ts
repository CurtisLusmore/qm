import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  build: {
    outDir: '../be/wwwroot',
    emptyOutDir: true,
  },
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:1713',
        changeOrigin: false,
        secure: false,
      },
    },
  }
})
