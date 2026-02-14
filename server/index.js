import express from 'express'
import cors from 'cors'
import 'dotenv/config'

const app = express()
app.use(cors())
app.use(express.json())

app.get('/api/health', (req, res) => {
  res.json({ status: 'ok', time: new Date().toISOString() })
})

app.get('/api/posts', (req, res) => {
  res.json([{ id: 1, text: 'Welcome to SocialMind (Node API)' }])
})

const port = process.env.PORT || 4000
app.listen(port, () => console.log(`API server listening on http://localhost:${port}`))
  console.log(`API server listening on http://localhost:${port}`),
);
