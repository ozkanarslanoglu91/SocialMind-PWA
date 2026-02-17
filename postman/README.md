# 📮 Postman Collection - SocialMind TikTok API

Bu klasörde SocialMind projesi için hazırlanmış Postman koleksiyonları bulunmaktadır.

## 📁 Dosyalar

1. **TikTok_API_Collection.json** - TikTok API v2 endpoint'lerinin tamamı
2. **SocialMind_Development.postman_environment.json** - Development ortamı değişkenleri

## 🚀 Kurulum

### Postman'e Import Etme

1. **Postman Desktop** veya **Postman Web** açın
2. Sol üstteki **Import** butonuna tıklayın
3. **File** sekmesinden her iki JSON dosyasını seçin:
   - `TikTok_API_Collection.json`
   - `SocialMind_Development.postman_environment.json`
4. **Import** butonuna tıklayın

### Environment Ayarlama

1. Sağ üstten **Environments** açın
2. **SocialMind Development** seçin
3. Aşağıdaki değişkenleri doldurun:
   ```
   tiktokClientKey: your_actual_client_key
   tiktokClientSecret: your_actual_client_secret
   ```

## 🎯 Kullanım Senaryoları

### Senaryo 1: OAuth Flow Test

1. **OAuth** klasörü → **1. Get Authorization URL**
   - Run → Response'dan `auth_url` kopyala
   - Browser'da aç ve authorize et
   - Redirect URL'den `code` parametresini kopyala

2. **OAuth** klasörü → **2. Exchange Code for Token**
   - Environment'ta `authCode` değişkenine code'u yapıştır
   - Run → Access token otomatik kaydedilir

3. **User API** klasörü → **Get User Profile**
   - Run → Profilinizi görürsünüz

### Senaryo 2: Video Listeleme

1. OAuth flow'u tamamlayın (access token alın)
2. **User API** → **Get User Videos**
3. Query parametrelerini değiştirerek pagination test edin:
   - `max_count`: 1-20 arası
   - `cursor`: Pagination için

### Senaryo 3: Token Refresh Test

1. **OAuth** → **3. Refresh Access Token**
2. Response'dan yeni access token otomatik güncellenir
3. Diğer endpoint'leri yeni token ile test edin

### Senaryo 4: Video Upload Flow

1. **Video Upload** → **1. Initialize Upload**
   - Video bilgilerini request body'de ayarlayın
   - Upload URL otomatik kaydedilir

2. **Video Upload** → **2. Publish Video**
   - Video metadata'sını düzenleyin
   - Publish edin

## 🧪 Test Scripts

Her request'te otomatik test scriptleri çalışır:

- **Status code kontrolü**
- **Response validation**
- **Token otomasyonu** (access token otomatik environment'a kaydedilir)
- **Error handling**

### Test Sonuçlarını Görme

1. Request'i run edin
2. Alt panelde **Test Results** sekmesine bakın
3. ✅ Passed / ❌ Failed testleri görürsünüz

## 📊 Collection Runner

Tüm endpoint'leri sırayla test etmek için:

1. Collection'a sağ tıklayın → **Run collection**
2. Environment'ı seçin: **SocialMind Development**
3. İstediğiniz request'leri seçin
4. **Run SocialMind TikTok API** butonuna tıklayın

## 🔧 Environment Variables

| Variable | Type | Description |
|----------|------|-------------|
| `baseUrl` | default | API server URL (http://localhost:4000) |
| `tiktokClientKey` | secret | TikTok App Client Key |
| `tiktokClientSecret` | secret | TikTok App Client Secret |
| `accessToken` | secret | OAuth access token (auto-saved) |
| `refreshToken` | secret | OAuth refresh token (auto-saved) |
| `redirectUri` | default | OAuth redirect URI |
| `authCode` | default | Authorization code from OAuth |
| `uploadUrl` | default | Video upload URL (auto-saved) |

## 📝 Request Örnekleri

### Get User Profile
```http
GET {{baseUrl}}/api/social/tiktok/profile
Authorization: Bearer {{accessToken}}
```

### Publish Video
```json
POST {{baseUrl}}/api/social/tiktok/publish
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "post_info": {
    "title": "My TikTok Video",
    "privacy_level": "SELF_ONLY",
    "disable_comment": false,
    "disable_duet": false,
    "disable_stitch": false
  },
  "source_info": {
    "source": "FILE_UPLOAD",
    "video_url": "{{uploadUrl}}"
  }
}
```

## 🐛 Troubleshooting

### Problem: "Could not send request"
**Çözüm:** 
- Server'ın çalıştığından emin olun: `cd server && npm run dev`
- `baseUrl` değişkenini kontrol edin

### Problem: "401 Unauthorized"
**Çözüm:**
- Access token'ın geçerli olduğundan emin olun
- Token expired ise **Refresh Access Token** endpoint'ini kullanın

### Problem: "Invalid client_key"
**Çözüm:**
- Environment'ta `tiktokClientKey` ve `tiktokClientSecret` değerlerini kontrol edin
- TikTok Developer Portal'dan doğru değerleri kopyalayın

## 📚 Dökümantasyon

- [TikTok API Docs](https://developers.tiktok.com/doc)
- [SocialMind TikTok Setup Guide](../docs/TIKTOK_SETUP.md)
- [SocialMind TikTok Testing Guide](../docs/TIKTOK_TESTING.md)

## 🎨 Collection Features

- ✅ 9 hazır endpoint
- ✅ OAuth 2.0 flow (4 endpoint)
- ✅ User API (2 endpoint)
- ✅ Video Upload (2 endpoint)
- ✅ Otomatik test scripts
- ✅ Token yönetimi
- ✅ Error handling
- ✅ Request examples
- ✅ Response validation

## 💡 Pro Tips

1. **Variables kullanın** - Hard-coded değerler yerine `{{variable}}` kullanın
2. **Tests yazın** - Her request için validation logic ekleyin
3. **Documentation** - Request descriptions'ları doldurun
4. **Mock Server** - Development için mock server oluşturun
5. **Monitor** - Production'da API monitoring kurun
6. **Team Share** - Workspace'i takımla paylaşın

## 🚀 Postman MCP ile Daha Fazlası

Bu collection'ı Postman MCP sunucusu ile kullanarak:

- ✅ VS Code'dan direkt Postman API'leriyle çalışın
- ✅ Collection'ları programatik olarak yönetin
- ✅ Automated testing pipeline'ları kurun
- ✅ Mock server'lar oluşturun
- ✅ API monitoring setup'ı yapın

---

**Created for:** SocialMind Project  
**TikTok App ID:** 7600244017401530424  
**Version:** 1.0.0  
**Last Updated:** 17 Şubat 2026
