/**
 * TikTok API Client for Frontend
 * 
 * Usage Example:
 * 
 * import { TikTokClient } from '@/services/tiktok-client'
 * 
 * const client = new TikTokClient()
 * 
 * // Start OAuth flow
 * const authUrl = await client.getAuthorizationUrl()
 * window.location.href = authUrl
 * 
 * // After redirect, exchange code for token
 * const token = await client.handleCallback(code)
 * client.setAccessToken(token.access_token)
 * 
 * // Get user profile
 * const profile = await client.getUserProfile()
 * 
 * // Get user videos
 * const videos = await client.getUserVideos()
 */

import { TIKTOK_CONFIG } from '@/config/tiktok.config'

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:4000'

export interface TikTokAuthResponse {
  success: boolean
  auth_url: string
  state: string
}

export interface TikTokTokenResponse {
  success: boolean
  provider: string
  access_token: string
  expires_in: number
  refresh_token: string
  open_id: string
  scope: string
  state?: string
  error?: string
}

export interface TikTokUserInfo {
  success: boolean
  user?: {
    open_id: string
    union_id?: string
    avatar_url?: string
    display_name: string
    username?: string
    follower_count: number
    following_count: number
    likes_count: number
    video_count: number
  }
  error?: string
}

export interface TikTokVideo {
  id: string
  create_time: number
  cover_image_url: string
  share_url: string
  video_description: string
  duration: number
  height: number
  width: number
  title: string
  embed_html: string
  embed_link: string
  like_count: number
  comment_count: number
  share_count: number
  view_count: number
}

export interface TikTokVideosResponse {
  success: boolean
  videos?: TikTokVideo[]
  has_more: boolean
  cursor?: string
  error?: string
}

export interface TikTokUploadInitResponse {
  success: boolean
  publish_id?: string
  upload_url?: string
  message?: string
  error?: string
}

export interface TikTokPublishResponse {
  success: boolean
  publish_id?: string
  error?: string
}

export class TikTokClient {
  private accessToken: string | null = null
  private refreshToken: string | null = null

  constructor() {
    // Load tokens from localStorage if available
    this.loadTokensFromStorage()
  }

  /**
   * Get OAuth authorization URL
   */
  async getAuthorizationUrl(state?: string): Promise<string> {
    try {
      const response = await fetch(`${API_BASE}${TIKTOK_CONFIG.ENDPOINTS.AUTH}${state ? `?state=${state}` : ''}`)
      const data: TikTokAuthResponse = await response.json()
      
      if (!data.success) {
        throw new Error('Failed to get authorization URL')
      }
      
      return data.auth_url
    } catch (error) {
      console.error('TikTok getAuthorizationUrl error:', error)
      throw error
    }
  }

  /**
   * Exchange authorization code for access token
   */
  async handleCallback(code: string, state?: string): Promise<TikTokTokenResponse> {
    try {
      const url = `${API_BASE}${TIKTOK_CONFIG.ENDPOINTS.CALLBACK}?code=${code}${state ? `&state=${state}` : ''}`
      const response = await fetch(url)
      const data: TikTokTokenResponse = await response.json()
      
      if (!data.success) {
        throw new Error(data.error || 'Failed to exchange code for token')
      }
      
      // Save tokens
      this.setAccessToken(data.access_token)
      this.setRefreshToken(data.refresh_token)
      
      return data
    } catch (error) {
      console.error('TikTok handleCallback error:', error)
      throw error
    }
  }

  /**
   * Refresh access token
   */
  async refreshAccessToken(): Promise<TikTokTokenResponse> {
    if (!this.refreshToken) {
      throw new Error('No refresh token available')
    }

    try {
      const response = await fetch(`${API_BASE}${TIKTOK_CONFIG.ENDPOINTS.REFRESH}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ refresh_token: this.refreshToken })
      })
      
      const data: TikTokTokenResponse = await response.json()
      
      if (!data.success) {
        throw new Error(data.error || 'Failed to refresh token')
      }
      
      // Update tokens
      this.setAccessToken(data.access_token)
      this.setRefreshToken(data.refresh_token)
      
      return data
    } catch (error) {
      console.error('TikTok refreshAccessToken error:', error)
      throw error
    }
  }

  /**
   * Get user profile
   */
  async getUserProfile(): Promise<TikTokUserInfo> {
    this.ensureAuthenticated()

    try {
      const response = await fetch(`${API_BASE}${TIKTOK_CONFIG.ENDPOINTS.PROFILE}`, {
        headers: {
          'Authorization': `Bearer ${this.accessToken}`
        }
      })
      
      const data: TikTokUserInfo = await response.json()
      
      if (!data.success) {
        throw new Error(data.error || 'Failed to get user profile')
      }
      
      return data
    } catch (error) {
      console.error('TikTok getUserProfile error:', error)
      throw error
    }
  }

  /**
   * Get user videos
   */
  async getUserVideos(maxCount: number = 20): Promise<TikTokVideosResponse> {
    this.ensureAuthenticated()

    try {
      const response = await fetch(`${API_BASE}${TIKTOK_CONFIG.ENDPOINTS.VIDEOS}?max_count=${maxCount}`, {
        headers: {
          'Authorization': `Bearer ${this.accessToken}`
        }
      })
      
      const data: TikTokVideosResponse = await response.json()
      
      if (!data.success) {
        throw new Error(data.error || 'Failed to get user videos')
      }
      
      return data
    } catch (error) {
      console.error('TikTok getUserVideos error:', error)
      throw error
    }
  }

  /**
   * Initialize video upload
   */
  async initializeUpload(videoSize: number, chunkSize?: number): Promise<TikTokUploadInitResponse> {
    this.ensureAuthenticated()

    try {
      const response = await fetch(`${API_BASE}${TIKTOK_CONFIG.ENDPOINTS.UPLOAD_INIT}`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${this.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          video_size: videoSize,
          chunk_size: chunkSize || TIKTOK_CONFIG.VIDEO_CONSTRAINTS.CHUNK_SIZE
        })
      })
      
      const data: TikTokUploadInitResponse = await response.json()
      
      if (!data.success) {
        throw new Error(data.error || 'Failed to initialize upload')
      }
      
      return data
    } catch (error) {
      console.error('TikTok initializeUpload error:', error)
      throw error
    }
  }

  /**
   * Publish video
   */
  async publishVideo(
    publishId: string,
    postInfo: {
      title?: string
      privacy_level?: 'PUBLIC_TO_EVERYONE' | 'MUTUAL_FOLLOW_FRIENDS' | 'SELF_ONLY'
      disable_duet?: boolean
      disable_comment?: boolean
      disable_stitch?: boolean
      video_cover_timestamp_ms?: number
    }
  ): Promise<TikTokPublishResponse> {
    this.ensureAuthenticated()

    try {
      const response = await fetch(`${API_BASE}${TIKTOK_CONFIG.ENDPOINTS.PUBLISH}`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${this.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          publish_id: publishId,
          post_info: postInfo
        })
      })
      
      const data: TikTokPublishResponse = await response.json()
      
      if (!data.success) {
        throw new Error(data.error || 'Failed to publish video')
      }
      
      return data
    } catch (error) {
      console.error('TikTok publishVideo error:', error)
      throw error
    }
  }

  /**
   * Revoke access token (logout)
   */
  async revokeToken(): Promise<boolean> {
    this.ensureAuthenticated()

    try {
      const response = await fetch(`${API_BASE}${TIKTOK_CONFIG.ENDPOINTS.REVOKE}`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${this.accessToken}`
        }
      })
      
      const data = await response.json()
      
      if (data.success) {
        this.clearTokens()
      }
      
      return data.success
    } catch (error) {
      console.error('TikTok revokeToken error:', error)
      throw error
    }
  }

  /**
   * Set access token
   */
  setAccessToken(token: string): void {
    this.accessToken = token
    localStorage.setItem('tiktok_access_token', token)
  }

  /**
   * Set refresh token
   */
  setRefreshToken(token: string): void {
    this.refreshToken = token
    localStorage.setItem('tiktok_refresh_token', token)
  }

  /**
   * Get access token
   */
  getAccessToken(): string | null {
    return this.accessToken
  }

  /**
   * Check if authenticated
   */
  isAuthenticated(): boolean {
    return !!this.accessToken
  }

  /**
   * Clear tokens
   */
  clearTokens(): void {
    this.accessToken = null
    this.refreshToken = null
    localStorage.removeItem('tiktok_access_token')
    localStorage.removeItem('tiktok_refresh_token')
  }

  /**
   * Load tokens from localStorage
   */
  private loadTokensFromStorage(): void {
    this.accessToken = localStorage.getItem('tiktok_access_token')
    this.refreshToken = localStorage.getItem('tiktok_refresh_token')
  }

  /**
   * Ensure user is authenticated
   */
  private ensureAuthenticated(): void {
    if (!this.accessToken) {
      throw new Error('Not authenticated. Please login first.')
    }
  }
}

export default TikTokClient
