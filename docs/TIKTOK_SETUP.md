# TikTok API Entegrasyon Rehberi

## 📱 TikTok Developer Kurulumu

SocialMind projenizde TikTok entegrasyonunu aktif etmek için aşağıdaki adımları takip edin.

### 1. TikTok Developer Hesabı Oluşturma

1. [TikTok Developer Portal](https://developers.tiktok.com/) adresine gidin
2. TikTok hesabınızla giriş yapın
3. "Create New App" butonuna tıklayın

### 2. Uygulama Ayarları

#### Temel Bilgiler
- **App Name**: SocialMind (veya istediğiniz isim)
- **App ID**: `7600244017401530424` (zaten oluşturulmuş)
- **App Type**: Website
- **Website URL**: `http://localhost:5173` (development için)

#### Redirect URI Ayarları

TikTok Developer Portal'da "Login Kit" bölümüne gidin ve aşağıdaki Redirect URI'leri ekleyin:

**Development:**
```
http://localhost:5173/auth/tiktok/callback
```

**Production:**
```
https://yourdomain.com/auth/tiktok/callback
```

#### Gerekli Scope'lar

TikTok API'sinden aşağıdaki izinleri talep edin:

- ✅ `user.info.basic` - Kullanıcı temel bilgileri (zorunlu)
- ✅ `video.list` - Kullanıcının videolarını listeleme
- ✅ `video.upload` - Video yükleme ve yayınlama

### 3. API Credentials

TikTok Developer Portal'dan aşağıdaki bilgileri alın:

1. **Client Key** (Client ID)
2. **Client Secret**

### 4. Environment Variables (.env)

`.env` dosyanızı oluşturun ve aşağıdaki değişkenleri ekleyin:

```env
# TikTok API Credentials
TIKTOK_CLIENT_KEY=your_client_key_from_tiktok_portal
TIKTOK_CLIENT_SECRET=your_client_secret_from_tiktok_portal
TIKTOK_REDIRECT_URI=http://localhost:5173/auth/tiktok/callback
```

**⚠️ Önemli:** Client Secret'ı asla public repository'de paylaşmayın!

## 🚀 Kurulum ve Kullanım

### Backend Server'ı Başlatma

```bash
cd server
npm install
npm run dev
```

Server `http://localhost:4000` adresinde çalışacaktır.

### Frontend'i Başlatma

```bash
npm install
npm run dev
```

Frontend `http://localhost:5173` adresinde çalışacaktır.

## 📚 API Kullanımı

### JavaScript/TypeScript Örneği

```typescript
import { TikTokClient } from '@/services/tiktok-client'

// 1. Client oluştur
const client = new TikTokClient()

// 2. OAuth akışını başlat
const authUrl = await client.getAuthorizationUrl()
window.location.href = authUrl

// 3. Callback'ten dönen code ile token al
// URL: /auth/tiktok/callback?code=xxx&state=xxx
const urlParams = new URLSearchParams(window.location.search)
const code = urlParams.get('code')

if (code) {
  const tokenData = await client.handleCallback(code)
  console.log('Access Token:', tokenData.access_token)
  
  // 4. Kullanıcı bilgilerini al
  const profile = await client.getUserProfile()
  console.log('User:', profile.user)
  
  // 5. Kullanıcının videolarını listele
  const videos = await client.getUserVideos(10)
  console.log('Videos:', videos.videos)
}
```

### React Component Örneği

```tsx
import { useState } from 'react'
import { TikTokClient } from '@/services/tiktok-client'

export function TikTokConnect() {
  const [client] = useState(() => new TikTokClient())
  const [profile, setProfile] = useState(null)

  const handleConnect = async () => {
    const authUrl = await client.getAuthorizationUrl()
    window.location.href = authUrl
  }

  const handleCallback = async (code: string) => {
    await client.handleCallback(code)
    const userProfile = await client.getUserProfile()
    setProfile(userProfile.user)
  }

  return (
    <div>
      {!client.isAuthenticated() ? (
        <button onClick={handleConnect}>
          Connect TikTok
        </button>
      ) : (
        <div>
          <h3>Connected as {profile?.display_name}</h3>
          <p>Followers: {profile?.follower_count}</p>
        </div>
      )}
    </div>
  )
}
```

## 🔌 API Endpoints

### Authentication

#### GET `/api/social/tiktok/auth`
OAuth authorization URL al

**Response:**
```json
{
  "success": true,
  "auth_url": "https://www.tiktok.com/v2/auth/authorize/...",
  "state": "random_state_123"
}
```

#### GET `/api/social/tiktok/callback?code=xxx`
Authorization code'u token'a çevir

**Response:**
```json
{
  "success": true,
  "provider": "tiktok",
  "access_token": "act.xxx",
  "refresh_token": "rft.xxx",
  "expires_in": 86400,
  "open_id": "user_open_id",
  "scope": "user.info.basic,video.list"
}
```

### User API

#### GET `/api/social/tiktok/profile`
Kullanıcı bilgilerini al

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "user": {
    "open_id": "xxx",
    "display_name": "John Doe",
    "avatar_url": "https://...",
    "follower_count": 1234,
    "following_count": 567,
    "video_count": 89,
    "likes_count": 12345
  }
}
```

#### GET `/api/social/tiktok/videos?max_count=20`
Kullanıcının videolarını listele

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "videos": [
    {
      "id": "video_123",
      "title": "My Video",
      "create_time": 1234567890,
      "cover_image_url": "https://...",
      "share_url": "https://tiktok.com/@user/video/123",
      "like_count": 100,
      "comment_count": 20,
      "share_count": 5,
      "view_count": 1000
    }
  ],
  "has_more": false
}
```

### Video Upload API

#### POST `/api/social/tiktok/upload/init`
Video yükleme oturumu başlat

**Headers:**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Body:**
```json
{
  "video_size": 12345678,
  "chunk_size": 5242880
}
```

**Response:**
```json
{
  "success": true,
  "publish_id": "pub_123",
  "upload_url": "https://upload.tiktok.com/..."
}
```

#### POST `/api/social/tiktok/publish`
Yüklenen videoyu yayınla

**Headers:**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Body:**
```json
{
  "publish_id": "pub_123",
  "post_info": {
    "title": "My Amazing Video",
    "privacy_level": "PUBLIC_TO_EVERYONE",
    "disable_duet": false,
    "disable_comment": false,
    "disable_stitch": false,
    "video_cover_timestamp_ms": 1000
  }
}
```

**Response:**
```json
{
  "success": true,
  "publish_id": "pub_123"
}
```

## 📊 Rate Limits

TikTok API rate limit'leri:

- **User Info**: 100 istek/gün
- **Video List**: 100 istek/gün
- **Video Upload**: 50 video/gün
- **Video Publish**: 50 video/gün

## 🎥 Video Gereksinimleri

### Desteklenen Formatlar
- MP4
- MOV
- AVI
- FLV
- WebM

### Boyut Limitleri
- **Minimum Boyut**: 1 KB
- **Maximum Boyut**: 4 GB
- **Minimum Süre**: 3 saniye
- **Maximum Süre**: 10 dakika

### Chunk Upload
- **Chunk Size**: 5 MB (varsayılan)
- Video büyük ise chunk'lar halinde yüklenir

## 🔐 Güvenlik

### Token Yönetimi
- Access token'lar 24 saat geçerlidir
- Refresh token'ları güvenli bir şekilde saklayın
- Token'ları asla client-side'da plain text olarak saklamayın
- Üretim ortamında HTTPS kullanın

### Best Practices
1. Token'ları localStorage yerine secure HTTP-only cookie'lerde saklayın
2. CSRF koruması için state parametresini kullanın
3. API isteklerinde her zaman rate limit'leri kontrol edin
4. Hata durumlarını loglayın ve kullanıcıya anlamlı mesajlar gösterin

## 🐛 Troubleshooting

### "Invalid Client Key" Hatası
- Client Key'i doğru kopyaladığınızdan emin olun
- TikTok Developer Portal'da uygulamanızın aktif olduğunu kontrol edin

### "Redirect URI Mismatch" Hatası
- `.env` dosyasındaki `TIKTOK_REDIRECT_URI` ile TikTok Portal'daki URI'nin tam olarak eşleştiğinden emin olun
- HTTP/HTTPS farkına dikkat edin

### "Insufficient Scope" Hatası
- TikTok Developer Portal'da gerekli scope'ların (user.info.basic, video.list, video.upload) aktif olduğundan emin olun

### Token Expired
- Refresh token kullanarak yeni access token alın:
```typescript
await client.refreshAccessToken()
```

## 📞 Destek

- **TikTok Developer Docs**: https://developers.tiktok.com/doc/overview
- **API Reference**: https://developers.tiktok.com/doc/login-kit-web
- **Community Forum**: https://developers.tiktok.com/community

## 📝 Lisans

Bu entegrasyon MIT lisansı altında SocialMind projesinin bir parçasıdır.
