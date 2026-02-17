/**
 * TikTok Connection Component
 * 
 * Bu component TikTok OAuth akışını yönetir ve kullanıcı bilgilerini gösterir.
 * 
 * Kullanım:
 * import { TikTokConnection } from '@/components/TikTokConnection'
 * 
 * function App() {
 *   return <TikTokConnection />
 * }
 */

import { useState, useEffect } from 'react'
import { TikTokClient } from '@/services/tiktok-client'
import type { TikTokUserInfo, TikTokVideo } from '@/services/tiktok-client'

export function TikTokConnection() {
  const [client] = useState(() => new TikTokClient())
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [profile, setProfile] = useState<TikTokUserInfo['user'] | null>(null)
  const [videos, setVideos] = useState<TikTokVideo[]>([])

  // Handle OAuth callback
  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search)
    const code = urlParams.get('code')
    const pathname = window.location.pathname

    if (code && pathname === '/auth/tiktok/callback') {
      handleCallback(code)
    } else if (client.isAuthenticated()) {
      loadUserData()
    }
  }, [])

  const handleCallback = async (code: string) => {
    setLoading(true)
    setError(null)
    
    try {
      await client.handleCallback(code)
      await loadUserData()
      
      // Redirect to home after successful auth
      window.history.replaceState({}, '', '/')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to authenticate')
    } finally {
      setLoading(false)
    }
  }

  const loadUserData = async () => {
    setLoading(true)
    setError(null)
    
    try {
      const [profileData, videosData] = await Promise.all([
        client.getUserProfile(),
        client.getUserVideos(10)
      ])
      
      setProfile(profileData.user || null)
      setVideos(videosData.videos || [])
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load user data')
    } finally {
      setLoading(false)
    }
  }

  const handleConnect = async () => {
    setLoading(true)
    setError(null)
    
    try {
      const authUrl = await client.getAuthorizationUrl()
      window.location.href = authUrl
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to start authentication')
      setLoading(false)
    }
  }

  const handleDisconnect = async () => {
    setLoading(true)
    setError(null)
    
    try {
      await client.revokeToken()
      setProfile(null)
      setVideos([])
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to disconnect')
    } finally {
      setLoading(false)
    }
  }

  const handleRefresh = async () => {
    setLoading(true)
    setError(null)
    
    try {
      await client.refreshAccessToken()
      await loadUserData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to refresh token')
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="bg-white rounded-lg shadow-lg p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold flex items-center gap-2">
            <svg className="w-8 h-8" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19.59 6.69a4.83 4.83 0 01-3.77-4.25V2h-3.45v13.67a2.89 2.89 0 01-5.2 1.74 2.89 2.89 0 012.31-4.64 2.93 2.93 0 01.88.13V9.4a6.84 6.84 0 00-1-.05A6.33 6.33 0 005 20.1a6.34 6.34 0 0010.86-4.43v-7a8.16 8.16 0 004.77 1.52v-3.4a4.85 4.85 0 01-1-.1z"/>
            </svg>
            TikTok Connection
          </h2>
          
          {client.isAuthenticated() && (
            <div className="flex gap-2">
              <button
                onClick={handleRefresh}
                className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition"
              >
                Refresh
              </button>
              <button
                onClick={handleDisconnect}
                className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 transition"
              >
                Disconnect
              </button>
            </div>
          )}
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded text-red-700">
            {error}
          </div>
        )}

        {!client.isAuthenticated() ? (
          <div className="text-center py-8">
            <p className="mb-4 text-gray-600">
              Connect your TikTok account to manage your content
            </p>
            <button
              onClick={handleConnect}
              className="px-6 py-3 bg-gradient-to-r from-cyan-500 to-pink-500 text-white font-semibold rounded-lg hover:shadow-lg transition"
            >
              Connect TikTok Account
            </button>
          </div>
        ) : profile ? (
          <div>
            {/* User Profile */}
            <div className="flex items-center gap-4 mb-6 p-4 bg-gray-50 rounded-lg">
              {profile.avatar_url && (
                <img
                  src={profile.avatar_url}
                  alt={profile.display_name}
                  className="w-16 h-16 rounded-full"
                />
              )}
              <div>
                <h3 className="text-xl font-semibold">{profile.display_name}</h3>
                {profile.username && (
                  <p className="text-gray-600">@{profile.username}</p>
                )}
              </div>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-4 gap-4 mb-6">
              <div className="text-center p-4 bg-gradient-to-br from-blue-50 to-blue-100 rounded-lg">
                <div className="text-2xl font-bold text-blue-600">
                  {profile.follower_count.toLocaleString()}
                </div>
                <div className="text-sm text-gray-600">Followers</div>
              </div>
              <div className="text-center p-4 bg-gradient-to-br from-purple-50 to-purple-100 rounded-lg">
                <div className="text-2xl font-bold text-purple-600">
                  {profile.following_count.toLocaleString()}
                </div>
                <div className="text-sm text-gray-600">Following</div>
              </div>
              <div className="text-center p-4 bg-gradient-to-br from-pink-50 to-pink-100 rounded-lg">
                <div className="text-2xl font-bold text-pink-600">
                  {profile.video_count.toLocaleString()}
                </div>
                <div className="text-sm text-gray-600">Videos</div>
              </div>
              <div className="text-center p-4 bg-gradient-to-br from-red-50 to-red-100 rounded-lg">
                <div className="text-2xl font-bold text-red-600">
                  {profile.likes_count.toLocaleString()}
                </div>
                <div className="text-sm text-gray-600">Likes</div>
              </div>
            </div>

            {/* Videos */}
            {videos.length > 0 && (
              <div>
                <h3 className="text-lg font-semibold mb-4">Recent Videos</h3>
                <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                  {videos.map((video) => (
                    <a
                      key={video.id}
                      href={video.share_url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="group relative aspect-[9/16] rounded-lg overflow-hidden bg-gray-100 hover:shadow-lg transition"
                    >
                      <img
                        src={video.cover_image_url}
                        alt={video.title || video.video_description}
                        className="w-full h-full object-cover"
                      />
                      <div className="absolute inset-0 bg-gradient-to-t from-black/70 to-transparent opacity-0 group-hover:opacity-100 transition">
                        <div className="absolute bottom-0 left-0 right-0 p-3">
                          <p className="text-white text-sm font-medium line-clamp-2">
                            {video.title || video.video_description}
                          </p>
                          <div className="flex gap-3 mt-2 text-white text-xs">
                            <span>❤️ {video.like_count.toLocaleString()}</span>
                            <span>💬 {video.comment_count.toLocaleString()}</span>
                            <span>👁️ {video.view_count.toLocaleString()}</span>
                          </div>
                        </div>
                      </div>
                    </a>
                  ))}
                </div>
              </div>
            )}
          </div>
        ) : null}
      </div>
    </div>
  )
}

export default TikTokConnection
