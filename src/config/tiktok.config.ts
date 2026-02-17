/**
 * TikTok API Configuration
 * 
 * Bu dosya TikTok Developer Portal'da oluşturduğunuz uygulamanın bilgilerini içerir.
 * 
 * TikTok Developer Portal: https://developers.tiktok.com/
 * App ID: 7600244017401530424
 * 
 * Gerekli Ayarlar:
 * 1. TikTok Developer Portal'da uygulamanızı oluşturun
 * 2. Redirect URI'yi ayarlayın (örn: http://localhost:5173/auth/tiktok/callback)
 * 3. Client Key ve Client Secret'ı .env dosyasına ekleyin
 * 4. Gerekli scope'ları onaylayın:
 *    - user.info.basic (Kullanıcı bilgileri)
 *    - video.list (Video listesi)
 *    - video.upload (Video yükleme)
 * 
 * OAuth Flow:
 * 1. Frontend: GET /api/social/tiktok/auth → Authorization URL al
 * 2. Kullanıcı TikTok'ta izin verir
 * 3. TikTok redirect yapar: /auth/tiktok/callback?code=xxx
 * 4. Frontend: GET /api/social/tiktok/callback?code=xxx → Access Token al
 * 5. Access Token ile API istekleri yap
 * 
 * API Endpoints:
 * - GET  /api/social/tiktok/auth            → OAuth URL
 * - GET  /api/social/tiktok/callback        → Token exchange
 * - POST /api/social/tiktok/refresh         → Token yenileme
 * - GET  /api/social/tiktok/profile         → Kullanıcı bilgileri
 * - GET  /api/social/tiktok/videos          → Kullanıcı videoları
 * - POST /api/social/tiktok/upload/init     → Video yükleme başlat
 * - POST /api/social/tiktok/publish         → Video yayınla
 * - POST /api/social/tiktok/revoke          → Token iptal et
 * 
 * Environment Variables (.env):
 * TIKTOK_CLIENT_KEY=your_client_key_here
 * TIKTOK_CLIENT_SECRET=your_client_secret_here
 * TIKTOK_REDIRECT_URI=http://localhost:5173/auth/tiktok/callback
 */

export const TIKTOK_CONFIG = {
  APP_ID: '7600244017401530424',
  API_VERSION: 'v2',
  
  // OAuth Scopes
  REQUIRED_SCOPES: [
    'user.info.basic',
    'video.list',
    'video.upload'
  ],
  
  // API Endpoints (Server-side)
  ENDPOINTS: {
    AUTH: '/api/social/tiktok/auth',
    CALLBACK: '/api/social/tiktok/callback',
    REFRESH: '/api/social/tiktok/refresh',
    PROFILE: '/api/social/tiktok/profile',
    VIDEOS: '/api/social/tiktok/videos',
    UPLOAD_INIT: '/api/social/tiktok/upload/init',
    PUBLISH: '/api/social/tiktok/publish',
    REVOKE: '/api/social/tiktok/revoke'
  },
  
  // Rate Limits (TikTok API)
  RATE_LIMITS: {
    USER_INFO: '100 requests per day',
    VIDEO_LIST: '100 requests per day',
    VIDEO_UPLOAD: '50 videos per day',
    VIDEO_PUBLISH: '50 videos per day'
  },
  
  // Video Upload Constraints
  VIDEO_CONSTRAINTS: {
    MAX_SIZE: 4 * 1024 * 1024 * 1024, // 4GB
    MIN_SIZE: 1024, // 1KB
    MAX_DURATION: 600, // 10 minutes
    MIN_DURATION: 3, // 3 seconds
    SUPPORTED_FORMATS: ['mp4', 'mov', 'avi', 'flv', 'webm'],
    CHUNK_SIZE: 5 * 1024 * 1024, // 5MB
    MAX_TITLE_LENGTH: 150,
    PRIVACY_LEVELS: ['PUBLIC_TO_EVERYONE', 'MUTUAL_FOLLOW_FRIENDS', 'SELF_ONLY']
  }
}

export default TIKTOK_CONFIG
