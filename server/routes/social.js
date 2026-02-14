import express from 'express'
const router = express.Router()

// OAuth placeholders - return redirect URLs and callback handlers.
const providers = ['youtube','instagram','facebook','tiktok']

router.get('/providers', (req, res) => {
  return res.json({ providers })
})

router.get('/:provider/auth', (req, res) => {
  const { provider } = req.params
  if (!providers.includes(provider)) return res.status(400).json({ error: 'unknown provider' })
  // In real app build provider OAuth URL
  const redirect = `${req.protocol}://${req.get('host')}/auth/${provider}/callback?code=demo`;
  return res.json({ auth_url: redirect })
})

router.get('/:provider/callback', (req, res) => {
  // placeholder: accept code and return demo token
  const { provider } = req.params
  const code = req.query.code || 'demo'
  return res.json({ provider, token: `demo-token-${provider}`, code })
})

router.get('/:provider/profile', (req, res) => {
  const { provider } = req.params
  return res.json({ provider, id: `user-${provider}`, name: `Demo ${provider}` })
})

export default router
