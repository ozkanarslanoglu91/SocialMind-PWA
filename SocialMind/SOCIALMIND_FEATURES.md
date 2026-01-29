# 🌐 SocialMind - Hybrid Sosyal Medya Yönetim Platformu

Tüm sosyal medya hesaplarınızı (YouTube, TikTok, Instagram, Facebook, Twitter/X, LinkedIn) SQL teknolojisi kullanarak **tek yerden yönetin**.

## ✨ Özellikler

### 📱 Desteklenen Platformlar
- ✅ YouTube (OAuth)
- ✅ TikTok (OAuth)
- ✅ Instagram (OAuth)
- ✅ Facebook (OAuth)
- ✅ Twitter/X (OAuth)
- ✅ LinkedIn (OAuth)

### 🤖 AI Model Entegrasyonları
- **OpenAI**: GPT-4o, GPT-4o Mini
- **Google Gemini**: 1.5 Pro, 1.5 Flash
- **NVIDIA**: Llama 3.1 Nemotron 70B, Mistral NeMo 12B
- **GitHub Models (ÜCRETSİZ!)**: 
  - GPT-4o & GPT-4o Mini
  - Phi-4
  - Llama 3.3 70B
  - Mistral Large
  - Cohere Command R
  - AI21 Jamba 1.5 (256K context)

### 📝 Post & İçerik Yönetimi
- Çok platformlu gönderi oluşturma
- Gerçek zamanlı platform önizlemeleri
- Akıllı karakter sayacı (platform özel limitler)
- Media yönetimi (resim, video)
- Hashtag ve mention desteği
- AI ile otomatik içerik üretimi

### ⏰ Zamanlama & Otomasyonu
- Belirli zaman planlaması
- Tekrarlayan gönderiler
- AI destekli optimal zamanlama önerileri
- Takvim görünümü
- Otomatik post yayını

### 📊 Analitik & İstatistikler
- Platform bazında performans metrikleri
- Post analitiği (likes, comments, shares, impressions)
- En iyi performans zamanları analizi
- Hashtag performans rakibi
- Kampanya ROI izleme
- Reklam yönetimi

### 🏗️ Teknik Yapı

#### Stack
- **Frontend**: Blazor (Server-side)
- **Backend**: ASP.NET Core 10
- **Framework**: .NET 10
- **Mobile**: .NET MAUI (iOS, Android, macOS, Windows)
- **Database**: SQL Server
- **Language**: C#
- **Package Manager**: NuGet

#### Proje Yapısı
```
SocialMind/
├── SocialMind/              # MAUI Mobile App
│   ├── App.xaml
│   ├── MainPage.xaml
│   └── Platforms/           # Platform-specific code
│
├── SocialMind.Web/          # ASP.NET Core Web App
│   ├── Components/
│   ├── Pages/
│   └── wwwroot/
│       └── assets/
│           ├── logos/       # Logolar
│           ├── icons/       # Platform ikonları
│           └── illustrations/
│
└── SocialMind.Shared/       # Shared Razor Components
    ├── Models/
    │   ├── AIModels.cs
    │   ├── PlatformModels.cs
    │   ├── PostModels.cs
    │   ├── AnalyticsModels.cs
    │   └── ScheduleModels.cs
    ├── Services/
    │   └── IServiceInterfaces.cs
    └── Components/
        └── SocialMind/
            ├── Dashboard.razor
            ├── PlatformSelector.razor
            ├── CharacterCounter.razor
            └── AIModelSelector.razor
```

## 🎨 Tasarım

### Renkler
- **Primary**: #6366f1 (Indigo)
- **Secondary**: #a855f7 (Purple)
- **Accent**: #ec4899 (Pink)
- **Error**: #dc2626 (Red)
- **Success**: #059669 (Green)

### Logolar & İkonlar
- **Main Logo**: `/assets/logos/socialmind-logo.svg` - Beyin + Sosyal bağlantılar
- **Platform Icons**: `/assets/icons/` - Her platform için renkli ikon
- **Illustrations**: `/assets/illustrations/empty-state.svg` - Boş durum görseli

## 🚀 Başlangıç

```bash
# Bağımlılıkları yükleme
dotnet restore

# Web uygulamasını çalıştırma
cd SocialMind.Web
dotnet run

# MAUI uygulamasını çalıştırma (Windows)
cd SocialMind
dotnet run -f net10.0-windows10.0.19041.0
```

## 📦 Model & Servis Yapısı

### Models
- `AIModels.cs` - AI provider'ları, modelleri ve konfigürasyonları
- `PlatformModels.cs` - Sosyal medya platformları ve bağlantılı hesaplar
- `PostModels.cs` - Post, media ve platform özel veriler
- `AnalyticsModels.cs` - Analitik, kampanya ve perfor performans metrikleri
- `ScheduleModels.cs` - Zamanlama, tekrarlama ve takvim

### Services (Interfaces)
- `IAIService` - AI içerik üretimi ve analizi
- `IAIModelFactory` - AI provider factory
- `IPostService` - Post CRUD, yayın operasyonları
- `IPlatformService` - OAuth, hesap yönetimi
- `IAnalyticsService` - Analitik raporları
- `IScheduleService` - Post zamanlama
- `ICampaignService` - Kampanya yönetimi
- `ISettingsService` - API anahtarı ve tercih yönetimi

### Shared Components
- `Dashboard.razor` - Ana sayfa dashboard
- `PlatformSelector.razor` - Multi-select platform seçici
- `CharacterCounter.razor` - Platform bazında karakter sayacı
- `AIModelSelector.razor` - AI model seçimi ve karşılaştırması

## 🔐 Güvenlik
- Tarayıcı tabanlı API anahtar depolaması
- OAuth2 entegrasyonu
- Şifre korumalı ayarlar
- GDPR uyumlu istemci tarafı veri işleme

## 📚 Kaynaklar
- GitHub Modelleri Marketplace
- OpenAI API Dokümantasyonu
- Google Gemini API
- NVIDIA AI Foundry
- Microsoft .NET 10 Dokümantasyonu

## 📄 Lisans

MIT License - Açık kaynak ve ücretsiz kullanım

---

**Yapılmış:** Hybrid .NET Stack ile Modern Sosyal Medya Yönetim
