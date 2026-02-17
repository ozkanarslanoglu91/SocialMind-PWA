import express from 'express'
import tiktokService from '../services/tiktok.js'

const router = express.Router()

// OAuth providers
const providers = ['youtube','instagram','facebook','tiktok','twitter','linkedin']

router.get('/providers', (req, res) => {
  return res.json({ providers })
})

// ==================== TikTok Routes ====================

/**
 * Get TikTok OAuth authorization URL
 * GET /api/social/tiktok/auth
 */
router.get('/tiktok/auth', (req, res) => {
  try {
    const state = req.query.state || `tiktok_${Date.now()}`
    const authUrl = tiktokService.getAuthorizationUrl(state)
    return res.json({ 
      success: true,
      auth_url: authUrl,
      state: state 
    })
  } catch (error) {
    console.error('TikTok auth error:', error)
    return res.status(500).json({ 
      success: false,
      error: error.message 
    })
  }
})

/**
 * TikTok OAuth callback - exchange code for token
 * GET /api/social/tiktok/callback?code=xxx&state=xxx
 */
router.get('/tiktok/callback', async (req, res) => {
  try {
    const { code, state } = req.query
    
    if (!code) {
      return res.status(400).json({ 
        success: false,
        error: 'Authorization code is required' 
      })
    }

    const tokenResult = await tiktokService.getAccessToken(code)
    
    if (!tokenResult.success) {
      return res.status(400).json(tokenResult)
    }

    return res.json({
      success: true,
      provider: 'tiktok',
      access_token: tokenResult.access_token,
      expires_in: tokenResult.expires_in,
      refresh_token: tokenResult.refresh_token,
      open_id: tokenResult.open_id,
      scope: tokenResult.scope,
      state: state
    })
  } catch (error) {
    console.error('TikTok callback error:', error)
    return res.status(500).json({ 
      success: false,
      error: error.message 
    })
  }
})

/**
 * Refresh TikTok access token
 * POST /api/social/tiktok/refresh
 * Body: { refresh_token: 'xxx' }
 */
router.post('/tiktok/refresh', async (req, res) => {
  try {
    const { refresh_token } = req.body
    
    if (!refresh_token) {
      return res.status(400).json({ 
        success: false,
        error: 'Refresh token is required' 
      })
    }

    const result = await tiktokService.refreshAccessToken(refresh_token)
    return res.json(result)
  } catch (error) {
    console.error('TikTok refresh error:', error)
    return res.status(500).json({ 
      success: false,
      error: error.message 
    })
  }
})

/**
 * Get TikTok user profile
 * GET /api/social/tiktok/profile
 * Headers: Authorization: Bearer {access_token}
 */
router.get('/tiktok/profile', async (req, res) => {
  try {
    const authHeader = req.headers.authorization
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({ 
        success: false,
        error: 'Access token is required' 
      })
    }

    const accessToken = authHeader.substring(7)
    const result = await tiktokService.getUserInfo(accessToken)
    return res.json(result)
  } catch (error) {
    console.error('TikTok profile error:', error)
    return res.status(500).json({ 
      success: false,
      error: error.message 
    })
  }
})

/**
 * Get TikTok user videos
 * GET /api/social/tiktok/videos?max_count=20
 * Headers: Authorization: Bearer {access_token}
 */
router.get('/tiktok/videos', async (req, res) => {
  try {
    const authHeader = req.headers.authorization
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({ 
        success: false,
        error: 'Access token is required' 
      })
    }

    const accessToken = authHeader.substring(7)
    const maxCount = parseInt(req.query.max_count) || 20
    const result = await tiktokService.getUserVideos(accessToken, maxCount)
    return res.json(result)
  } catch (error) {
    console.error('TikTok videos error:', error)
    return res.status(500).json({ 
      success: false,
      error: error.message 
    })
  }
})

/**
 * Initialize video upload
 * POST /api/social/tiktok/upload/init
 * Headers: Authorization: Bearer {access_token}
 * Body: { video_size: 12345678, chunk_size: 5242880 }
 */
router.post('/tiktok/upload/init', async (req, res) => {
  try {
    const authHeader = req.headers.authorization
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({ 
        success: false,
        error: 'Access token is required' 
      })
    }

    const accessToken = authHeader.substring(7)
    const videoData = req.body
    
    if (!videoData.video_size) {
      return res.status(400).json({ 
        success: false,
        error: 'video_size is required' 
      })
    }

    const result = await tiktokService.uploadVideo(accessToken, videoData)
    return res.json(result)
  } catch (error) {
    console.error('TikTok upload init error:', error)
    return res.status(500).json({ 
      success: false,
      error: error.message 
    })
  }
})

/**
 * Publish uploaded video
 * POST /api/social/tiktok/publish
 * Headers: Authorization: Bearer {access_token}
 * Body: { publish_id: 'xxx', post_info: {...} }
 */
router.post('/tiktok/publish', async (req, res) => {
  try {
    const authHeader = req.headers.authorization
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({ 
        success: false,
        error: 'Access token is required' 
      })
    }

    const accessToken = authHeader.substring(7)
    const { publish_id, post_info } = req.body
    
    if (!publish_id) {
      return res.status(400).json({ 
        success: false,
        error: 'publish_id is required' 
      })
    }

    const result = await tiktokService.publishVideo(accessToken, publish_id, post_info || {})
    return res.json(result)
  } catch (error) {
    console.error('TikTok publish error:', error)
    return res.status(500).json({ 
      success: false,
      error: error.message 
    })
  }
})

/**
 * Revoke TikTok access token
 * POST /api/social/tiktok/revoke
 * Headers: Authorization: Bearer {access_token}
 */
router.post('/tiktok/revoke', async (req, res) => {
  try {
    const authHeader = req.headers.authorization
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({ 
        success: false,
        error: 'Access token is required' 
      })
    }

    const accessToken = authHeader.substring(7)
    const result = await tiktokService.revokeToken(accessToken)
    return res.json(result)
  } catch (error) {
    console.error('TikTok revoke error:', error)
    return res.status(500).json({ 
      success: false,
      error: error.message 
    })
  }
})

// ==================== Generic Routes ====================

router.get('/:provider/auth', (req, res) => {
  const { provider } = req.params
  if (!providers.includes(provider)) {
    return res.status(400).json({ error: 'unknown provider' })
  }
  
  // For non-TikTok providers (placeholder)
  const redirect = `${req.protocol}://${req.get('host')}/auth/${provider}/callback?code=demo`
  return res.json({ auth_url: redirect })
})

router.get('/:provider/callback', (req, res) => {
  const { provider } = req.params
  const code = req.query.code || 'demo'
  return res.json({ provider, token: `demo-token-${provider}`, code })
})

router.get('/:provider/profile', (req, res) => {
  const { provider } = req.params
  return res.json({ provider, id: `user-${provider}`, name: `Demo ${provider}` })
})

export default router
