import express from 'express'
import cors from 'cors'
import dotenv from 'dotenv'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'

// Load .env from parent directory
const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)
dotenv.config({ path: join(__dirname, '..', '.env') })

import aiRouter from './routes/ai.js'
import socialRouter from './routes/social.js'

const app = express()
app.use(cors())
app.use(express.json())

app.get('/api/health', (req, res) => {
  res.json({ status: 'ok', time: new Date().toISOString() })
})

app.get('/api/posts', (req, res) => {
  res.json([{ id: 1, text: 'Welcome to SocialMind (Node API)' }])
})

// Mount API routers
app.use('/api/ai', aiRouter)
app.use('/api/social', socialRouter)

const port = process.env.PORT || 4000
app.listen(port, () => console.log(`API server listening on http://localhost:${port}`))
