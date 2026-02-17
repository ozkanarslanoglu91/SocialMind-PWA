/**
 * TikTok OAuth & API Service
 * Documentation: https://developers.tiktok.com/doc/login-kit-web
 * App ID: 7600244017401530424
 */

const TIKTOK_CLIENT_KEY = process.env.TIKTOK_CLIENT_KEY
const TIKTOK_CLIENT_SECRET = process.env.TIKTOK_CLIENT_SECRET
const TIKTOK_REDIRECT_URI = process.env.TIKTOK_REDIRECT_URI || 'http://localhost:5000/auth/tiktok/callback'

const TIKTOK_AUTH_BASE = 'https://www.tiktok.com/v2/auth/authorize/'
const TIKTOK_TOKEN_URL = 'https://open.tiktokapis.com/v2/oauth/token/'
const TIKTOK_API_BASE = 'https://open.tiktokapis.com/v2'

/**
 * Generate TikTok OAuth authorization URL
 * @param {string} state - CSRF protection state parameter
 * @returns {string} Authorization URL
 */
export function getAuthorizationUrl(state = 'random_state_string') {
  const params = new URLSearchParams({
    client_key: TIKTOK_CLIENT_KEY,
    scope: 'user.info.basic,video.list,video.upload',
    response_type: 'code',
    redirect_uri: TIKTOK_REDIRECT_URI,
    state: state
  })
  
  return `${TIKTOK_AUTH_BASE}?${params.toString()}`
}

/**
 * Exchange authorization code for access token
 * @param {string} code - Authorization code from OAuth callback
 * @returns {Promise<Object>} Token response
 */
export async function getAccessToken(code) {
  try {
    const response = await fetch(TIKTOK_TOKEN_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Cache-Control': 'no-cache'
      },
      body: new URLSearchParams({
        client_key: TIKTOK_CLIENT_KEY,
        client_secret: TIKTOK_CLIENT_SECRET,
        code: code,
        grant_type: 'authorization_code',
        redirect_uri: TIKTOK_REDIRECT_URI
      })
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(`TikTok token exchange failed: ${error}`)
    }

    const data = await response.json()
    return {
      success: true,
      access_token: data.access_token,
      expires_in: data.expires_in,
      refresh_token: data.refresh_token,
      open_id: data.open_id,
      scope: data.scope,
      token_type: data.token_type
    }
  } catch (error) {
    console.error('TikTok getAccessToken error:', error)
    return {
      success: false,
      error: error.message
    }
  }
}

/**
 * Refresh access token
 * @param {string} refreshToken - Refresh token
 * @returns {Promise<Object>} New token response
 */
export async function refreshAccessToken(refreshToken) {
  try {
    const response = await fetch(TIKTOK_TOKEN_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Cache-Control': 'no-cache'
      },
      body: new URLSearchParams({
        client_key: TIKTOK_CLIENT_KEY,
        client_secret: TIKTOK_CLIENT_SECRET,
        grant_type: 'refresh_token',
        refresh_token: refreshToken
      })
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(`TikTok token refresh failed: ${error}`)
    }

    const data = await response.json()
    return {
      success: true,
      access_token: data.access_token,
      expires_in: data.expires_in,
      refresh_token: data.refresh_token
    }
  } catch (error) {
    console.error('TikTok refreshAccessToken error:', error)
    return {
      success: false,
      error: error.message
    }
  }
}

/**
 * Get user info
 * @param {string} accessToken - Access token
 * @returns {Promise<Object>} User info
 */
export async function getUserInfo(accessToken) {
  try {
    const response = await fetch(`${TIKTOK_API_BASE}/user/info/?fields=open_id,union_id,avatar_url,display_name,username,follower_count,following_count,likes_count,video_count`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json'
      }
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(`TikTok getUserInfo failed: ${error}`)
    }

    const data = await response.json()
    return {
      success: true,
      user: data.data.user
    }
  } catch (error) {
    console.error('TikTok getUserInfo error:', error)
    return {
      success: false,
      error: error.message
    }
  }
}

/**
 * Get user videos
 * @param {string} accessToken - Access token
 * @param {number} max_count - Maximum number of videos to return (default: 20, max: 20)
 * @returns {Promise<Object>} Videos list
 */
export async function getUserVideos(accessToken, max_count = 20) {
  try {
    const response = await fetch(`${TIKTOK_API_BASE}/video/list/?fields=id,create_time,cover_image_url,share_url,video_description,duration,height,width,title,embed_html,embed_link,like_count,comment_count,share_count,view_count&max_count=${max_count}`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json'
      }
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(`TikTok getUserVideos failed: ${error}`)
    }

    const data = await response.json()
    return {
      success: true,
      videos: data.data.videos,
      has_more: data.data.has_more,
      cursor: data.data.cursor
    }
  } catch (error) {
    console.error('TikTok getUserVideos error:', error)
    return {
      success: false,
      error: error.message
    }
  }
}

/**
 * Upload video to TikTok
 * @param {string} accessToken - Access token
 * @param {Object} videoData - Video upload data
 * @returns {Promise<Object>} Upload result
 */
export async function uploadVideo(accessToken, videoData) {
  try {
    // Step 1: Initialize upload
    const initResponse = await fetch(`${TIKTOK_API_BASE}/post/publish/inbox/video/init/`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        source_info: {
          source: 'FILE_UPLOAD',
          video_size: videoData.video_size,
          chunk_size: videoData.chunk_size || 5242880, // 5MB default
          total_chunk_count: Math.ceil(videoData.video_size / (videoData.chunk_size || 5242880))
        }
      })
    })

    if (!initResponse.ok) {
      const error = await initResponse.text()
      throw new Error(`TikTok upload init failed: ${error}`)
    }

    const initData = await initResponse.json()
    const publishId = initData.data.publish_id
    const uploadUrl = initData.data.upload_url

    return {
      success: true,
      publish_id: publishId,
      upload_url: uploadUrl,
      message: 'Upload initialized. Use the upload_url to upload video chunks.'
    }
  } catch (error) {
    console.error('TikTok uploadVideo error:', error)
    return {
      success: false,
      error: error.message
    }
  }
}

/**
 * Publish uploaded video
 * @param {string} accessToken - Access token
 * @param {string} publishId - Publish ID from upload init
 * @param {Object} postInfo - Post information
 * @returns {Promise<Object>} Publish result
 */
export async function publishVideo(accessToken, publishId, postInfo) {
  try {
    const response = await fetch(`${TIKTOK_API_BASE}/post/publish/video/init/`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        post_info: {
          title: postInfo.title || '',
          privacy_level: postInfo.privacy_level || 'SELF_ONLY', // PUBLIC_TO_EVERYONE, MUTUAL_FOLLOW_FRIENDS, SELF_ONLY
          disable_duet: postInfo.disable_duet || false,
          disable_comment: postInfo.disable_comment || false,
          disable_stitch: postInfo.disable_stitch || false,
          video_cover_timestamp_ms: postInfo.video_cover_timestamp_ms || 1000
        },
        source_info: {
          source: 'FILE_UPLOAD',
          publish_id: publishId
        }
      })
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(`TikTok publishVideo failed: ${error}`)
    }

    const data = await response.json()
    return {
      success: true,
      publish_id: data.data.publish_id
    }
  } catch (error) {
    console.error('TikTok publishVideo error:', error)
    return {
      success: false,
      error: error.message
    }
  }
}

/**
 * Revoke access token
 * @param {string} accessToken - Access token to revoke
 * @returns {Promise<Object>} Revoke result
 */
export async function revokeToken(accessToken) {
  try {
    const response = await fetch(`${TIKTOK_API_BASE}/oauth/revoke/`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      },
      body: new URLSearchParams({
        client_key: TIKTOK_CLIENT_KEY,
        client_secret: TIKTOK_CLIENT_SECRET,
        token: accessToken
      })
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(`TikTok token revocation failed: ${error}`)
    }

    return {
      success: true,
      message: 'Token revoked successfully'
    }
  } catch (error) {
    console.error('TikTok revokeToken error:', error)
    return {
      success: false,
      error: error.message
    }
  }
}

export default {
  getAuthorizationUrl,
  getAccessToken,
  refreshAccessToken,
  getUserInfo,
  getUserVideos,
  uploadVideo,
  publishVideo,
  revokeToken
}
