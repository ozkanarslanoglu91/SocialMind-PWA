import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { resolve } from 'path'
import fs from 'fs'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    open: true,
    strictPort: false,
  },
  build: {
    target: 'ES2020',
    minify: 'terser',
    sourcemap: false,
    outDir: 'dist',
    assetsDir: 'assets',
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
        },
      },
    },
  },
  preview: {
    port: 4173,
    strictPort: false,
  },
  // Copy service worker to dist
  setupMiddlewares: (middlewares, { config }) => {
    config.build = config.build || {}
    return middlewares
  },
})

// Hook to copy sw.js to dist
const copySwOnBuild = {
  name: 'copy-sw',
  writeBundle() {
    const swSource = 'sw.js'
    const swDest = 'dist/sw.js'
    
    if (fs.existsSync(swSource)) {
      fs.copyFileSync(swSource, swDest)
      console.log(`✓ Service Worker copied: ${swDest}`)
    }
  },
}

export default defineConfig({
  plugins: [react(), copySwOnBuild],
  // ... rest of config
})
