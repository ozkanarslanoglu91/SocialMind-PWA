import express from 'express'
const router = express.Router()

// Simple OpenAI proxy (placeholder). Expects OPENAI_API_KEY in env.
router.post('/generate', async (req, res) => {
  const { prompt } = req.body || {}
  if (!process.env.OPENAI_API_KEY) return res.status(500).json({ error: 'OPENAI_API_KEY not set' })
  if (!prompt) return res.status(400).json({ error: 'prompt required' })

  try {
    const resp = await fetch('https://api.openai.com/v1/chat/completions', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${process.env.OPENAI_API_KEY}`,
      },
      body: JSON.stringify({ model: 'gpt-4o-mini', messages: [{ role: 'user', content: prompt }], max_tokens: 300 }),
    })
    const data = await resp.json()
    return res.json({ ok: true, data })
  } catch (e) {
    return res.status(500).json({ error: e.message })
  }
})

export default router
