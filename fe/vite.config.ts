import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../be/wwwroot',
    emptyOutDir: true,
  },
  server: {
    host: true,
  },
})
