import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    // Disable source maps in production (prevents seeing original source code)
    sourcemap: false,
    // Minify output
    minify: 'esbuild',
  },
})
