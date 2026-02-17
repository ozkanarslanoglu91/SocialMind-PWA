# TikTok Entegrasyon Test Rehberi

## 🧪 Test Adımları

### 1. Ön Hazırlık

```bash
# Gerekli bağımlılıkları yükle
npm install

# Server için bağımlılıkları yükle
cd server
npm install
cd ..
```

### 2. Environment Variables Ayarla

`.env` dosyanızı oluşturun ve TikTok credentials'ları ekleyin:

```env
TIKTOK_CLIENT_KEY=your_actual_client_key
TIKTOK_CLIENT_SECRET=your_actual_client_secret
TIKTOK_REDIRECT_URI=http://localhost:5173/auth/tiktok/callback
```

### 3. Server'ı Başlat

Terminal 1:
```bash
cd server
npm run dev
```

Server `http://localhost:4000` adresinde çalışmalı.

### 4. Frontend'i Başlat

Terminal 2:
```bash
npm run dev
```

Frontend `http://localhost:5173` adresinde çalışmalı.

### 5. Manuel Test

#### Test 1: OAuth Authorization URL
```bash
curl http://localhost:4000/api/social/tiktok/auth
```

Beklenen Response:
```json
{
  "success": true,
  "auth_url": "https://www.tiktok.com/v2/auth/authorize/...",
  "state": "tiktok_..."
}
```

#### Test 2: Providers Listesi
```bash
curl http://localhost:4000/api/social/providers
```

Beklenen Response:
```json
{
  "providers": ["youtube", "instagram", "facebook", "tiktok", "twitter", "linkedin"]
}
```

#### Test 3: Token Exchange (Mock)
```bash
curl "http://localhost:4000/api/social/tiktok/callback?code=test_code_123"
```

**Not:** Gerçek token almak için TikTok OAuth akışını tamamlamanız gerekir.

### 6. Browser'da Test

1. `http://localhost:5173` adresine gidin
2. TikTok Connection bileşenini görmelisiniz
3. "Connect TikTok Account" butonuna tıklayın
4. TikTok'a yönlendirileceksiniz
5. İzinleri onaylayın
6. `/auth/tiktok/callback` adresine geri döneceksiniz
7. Kullanıcı profili ve videolar görüntülenecek

### 7. API Endpoint Test Scriptleri

#### Test Script 1: Authorization Flow

`test-tiktok-auth.js` dosyası oluşturun:

```javascript
const BASE_URL = 'http://localhost:4000'

async function testAuthFlow() {
  console.log('🧪 Testing TikTok Authorization Flow...\n')
  
  // Step 1: Get auth URL
  console.log('📝 Step 1: Getting authorization URL...')
  const authResponse = await fetch(`${BASE_URL}/api/social/tiktok/auth`)
  const authData = await authResponse.json()
  console.log('✅ Auth URL:', authData.auth_url)
  console.log('✅ State:', authData.state)
  console.log('\n')
  
  console.log('▶️  Next: Visit the auth URL in browser and authorize the app')
  console.log('▶️  After authorization, you will be redirected to callback URL with code')
  console.log('▶️  Use that code in the next test script\n')
}

testAuthFlow().catch(console.error)
```

Çalıştır:
```bash
node test-tiktok-auth.js
```

#### Test Script 2: User Profile (Auth gerekli)

`test-tiktok-profile.js` dosyası oluşturun:

```javascript
const BASE_URL = 'http://localhost:4000'
const ACCESS_TOKEN = 'your_access_token_here' // OAuth'tan aldığınız token

async function testUserProfile() {
  console.log('🧪 Testing TikTok User Profile...\n')
  
  try {
    const response = await fetch(`${BASE_URL}/api/social/tiktok/profile`, {
      headers: {
        'Authorization': `Bearer ${ACCESS_TOKEN}`
      }
    })
    
    const data = await response.json()
    
    if (data.success) {
      console.log('✅ User Profile:')
      console.log('  - Display Name:', data.user.display_name)
      console.log('  - Username:', data.user.username)
      console.log('  - Followers:', data.user.follower_count)
      console.log('  - Following:', data.user.following_count)
      console.log('  - Videos:', data.user.video_count)
      console.log('  - Likes:', data.user.likes_count)
    } else {
      console.error('❌ Failed:', data.error)
    }
  } catch (error) {
    console.error('❌ Error:', error.message)
  }
}

testUserProfile().catch(console.error)
```

Çalıştır:
```bash
node test-tiktok-profile.js
```

### 8. Component Test

React component'i test etmek için:

1. `src/App.tsx` dosyasını düzenleyin:

```tsx
import { TikTokConnection } from '@/components/TikTokConnection'

function App() {
  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <TikTokConnection />
    </div>
  )
}

export default App
```

2. Browser'da `http://localhost:5173` adresine gidin

3. Beklenen görünüm:
   - TikTok logo ve başlık
   - "Connect TikTok Account" butonu
   - Butona tıklandığında TikTok OAuth'a yönlendirme
   - Callback'ten sonra profil bilgileri ve videolar

### 9. Error Handling Test

#### Test Invalid Token
```bash
curl -H "Authorization: Bearer invalid_token" \
  http://localhost:4000/api/social/tiktok/profile
```

Beklenen: 401 veya error response

#### Test Missing Token
```bash
curl http://localhost:4000/api/social/tiktok/profile
```

Beklenen: 401 Unauthorized

#### Test Invalid Code
```bash
curl "http://localhost:4000/api/social/tiktok/callback?code=invalid_code"
```

Beklenen: Error response from TikTok

## ✅ Test Checklist

- [ ] Server başlatıldı ve çalışıyor
- [ ] Frontend başlatıldı ve çalışıyor
- [ ] `/api/social/tiktok/auth` endpoint çalışıyor
- [ ] OAuth URL doğru format
- [ ] TikTok OAuth sayfası açılıyor
- [ ] Callback URL'e yönlendirme çalışıyor
- [ ] Token exchange başarılı
- [ ] User profile API çalışıyor
- [ ] User videos API çalışıyor
- [ ] React component render ediliyor
- [ ] Profil bilgileri gösteriliyor
- [ ] Videolar listeleniyor
- [ ] Disconnect butonu çalışıyor
- [ ] Refresh token çalışıyor
- [ ] Error handling doğru çalışıyor
- [ ] LocalStorage'da token saklanıyor
- [ ] Sayfa yenilendiğinde oturum devam ediyor

## 🐛 Common Issues

### Issue 1: "Cannot find module '@/services/tiktok-client'"

**Solution:**
`tsconfig.json` dosyasında path alias'ların tanımlı olduğundan emin olun:

```json
{
  "compilerOptions": {
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

### Issue 2: "CORS Error"

**Solution:**
Server'da CORS ayarlarını kontrol edin:

```javascript
// server/index.js
import cors from 'cors'
app.use(cors({
  origin: 'http://localhost:5173',
  credentials: true
}))
```

### Issue 3: "Redirect URI Mismatch"

**Solution:**
- TikTok Developer Portal'daki Redirect URI ile `.env` dosyasındakinin aynı olduğundan emin olun
- HTTP/HTTPS farkına dikkat edin
- Port numaralarını kontrol edin

### Issue 4: "Invalid Client Key or Secret"

**Solution:**
- TikTok Developer Portal'dan doğru Client Key ve Secret'ı kopyalayın
- `.env` dosyasında string olarak (tırnak işareti olmadan) yazın
- Server'ı yeniden başlatın

## 📊 Success Metrics

✅ **Başarılı Entegrasyon Kriterleri:**
1. OAuth akışı sorunsuz tamamlanıyor
2. Kullanıcı profili gösteriliyor
3. Videolar listeleniyor
4. Token refresh çalışıyor
5. Disconnect/revoke çalışıyor
6. Error handling doğru çalışıyor
7. UI responsive ve kullanıcı dostu

## 🚀 Production Checklist

Üretime almadan önce:

- [ ] Environment variables production ortamına eklendi
- [ ] Redirect URI production domain'e güncellendi
- [ ] HTTPS zorunlu kılındı
- [ ] Rate limiting implementasyonu eklendi
- [ ] Error logging/monitoring kuruldu
- [ ] Token encryption eklendi
- [ ] CSRF protection aktif
- [ ] API keys güvenli bir şekilde saklanıyor
- [ ] User consent/privacy politikası hazır
- [ ] TikTok app review tamamlandı (eğer gerekiyorsa)

## 📝 Notes

- TikTok API rate limit'lerine dikkat edin
- Access token'lar 24 saat geçerlidir
- Refresh token'ları kullanarak otomatik yenileme yapın
- Video upload büyük dosyalar için chunk'lar halinde yapılmalı
- Test için gerçek bir TikTok hesabı kullanın
