# SocialMind Automation Scripts

Bu dizin SocialMind projesinin otomasyonunu sağlayan PowerShell scriptlerini içerir.

## 📜 Scriptler

### 🔨 build-all.ps1
Tüm platformlar için build işlemlerini otomatize eder.

```powershell
# Tüm platformları Debug modunda build et
.\build-all.ps1

# Sadece Web uygulamasını Release modunda build et
.\build-all.ps1 -Configuration Release -Platform Web

# Windows uygulamasını testler olmadan build et
.\build-all.ps1 -Platform Windows -SkipTests

# Paralel build (daha hızlı)
.\build-all.ps1 -Parallel
```

**Özellikleri:**
- ✅ Multi-platform support (Web, Windows, Android, iOS, MacCatalyst)
- ✅ NuGet restore otomasyonu
- ✅ Renkli konsol çıktısı
- ✅ Test entegrasyonu
- ✅ Build sonuç özeti

---

### 🛠️ dev-setup.ps1
Development environment kurulumunu otomatize eder.

```powershell
# Tüm development ortamını hazırla
.\dev-setup.ps1
```

**Özellikleri:**
- ✅ Tool version kontrolü (.NET, Git, Node.js)
- ✅ MAUI workload kurulumu
- ✅ NuGet & NPM dependencies
- ✅ Environment variables setup
- ✅ Database migration
- ✅ Git hooks kurulumu
- ✅ VS Code extensions önerisi

---

### 🗄️ db-migrate.ps1
Database migration ve seed operasyonlarını yönetir.

```powershell
# Migration'ları uygula
.\db-migrate.ps1 -Action Update

# Yeni migration oluştur
.\db-migrate.ps1 -Action Create -MigrationName "AddNewTable"

# Son migration'ı geri al
.\db-migrate.ps1 -Action Rollback -Force

# Database backup al
.\db-migrate.ps1 -Action Backup

# Database'i sıfırla (TÜM DATA SİLİNİR!)
.\db-migrate.ps1 -Action Reset -Force

# Seed data ekle
.\db-migrate.ps1 -Action Seed

# Database durumunu göster
.\db-migrate.ps1 -Action Status
```

**Özellikleri:**
- ✅ EF Core migration yönetimi
- ✅ Otomatik backup (30 gün retention)
- ✅ Seed data ekleme
- ✅ Database reset
- ✅ Migration listesi

**Default Admin:**
- Email: admin@socialmind.com
- Password: Admin123!

---

### 🐳 docker-deploy.ps1
Docker container yönetimini otomatize eder.

```powershell
# Docker image build et
.\docker-deploy.ps1 -Action Build

# Container'ları başlat (foreground)
.\docker-deploy.ps1 -Action Up

# Container'ları başlat (background)
.\docker-deploy.ps1 -Action Up -Detached

# Production environment
.\docker-deploy.ps1 -Action Up -Environment Production -Detached

# Container'ları durdur
.\docker-deploy.ps1 -Action Down

# Restart
.\docker-deploy.ps1 -Action Restart

# Logları göster
.\docker-deploy.ps1 -Action Logs

# Tam temizlik (image + container + volume)
.\docker-deploy.ps1 -Action Clean

# Container durumunu göster
.\docker-deploy.ps1 -Action Status
```

**Özellikleri:**
- ✅ Docker & Docker Compose yönetimi
- ✅ Development/Production environment desteği
- ✅ Resource monitoring
- ✅ Log streaming
- ✅ Health checks

**Erişim URL'leri:**
- Web App: http://localhost:8080
- API: http://localhost:8080/api

---

### 🧪 run-tests.ps1
Test automation ve code coverage.

```powershell
# Tüm testleri çalıştır
.\run-tests.ps1

# Sadece unit testler
.\run-tests.ps1 -Type Unit

# Integration testler coverage ile
.\run-tests.ps1 -Type Integration -Coverage

# Belirli testleri filtrele
.\run-tests.ps1 -Filter "TestMethodName"

# Watch mode (değişiklikleri izle)
.\run-tests.ps1 -Watch

# Verbose output
.\run-tests.ps1 -Verbose
```

**Test Kategorileri:**
- `Unit` - Unit testler
- `Integration` - Integration testler
- `E2E` - End-to-end testler
- `Performance` - Performance testler

**Özellikleri:**
- ✅ Test category filtering
- ✅ Code coverage (XPlat)
- ✅ HTML coverage report
- ✅ Watch mode
- ✅ TRX & HTML output

---

## 🚀 Hızlı Başlangıç

### 1️⃣ İlk Kurulum
```powershell
# Development environment hazırla
.\scripts\dev-setup.ps1

# Database'i hazırla
.\scripts\db-migrate.ps1 -Action Update
```

### 2️⃣ Development
```powershell
# Projeyi build et
.\scripts\build-all.ps1 -Platform Web

# Testleri çalıştır
.\scripts\run-tests.ps1 -Coverage
```

### 3️⃣ Docker Deployment
```powershell
# Docker build
.\scripts\docker-deploy.ps1 -Action Build

# Container başlat
.\scripts\docker-deploy.ps1 -Action Up -Detached
```

---

## 📋 Gereksinimler

- **PowerShell**: 7.0+
- **.NET SDK**: 10.0+
- **Docker**: 20.10+ (opsiyonel)
- **Git**: 2.30+
- **Node.js**: 24.x+ (opsiyonel)

---

## 🔧 Kurulum Notları

### Windows
Scriptler Windows'ta doğrudan çalışır:
```powershell
.\dev-setup.ps1
```

### Linux/macOS
PowerShell Core yükleyin:
```bash
# Linux
sudo apt-get install -y powershell

# macOS
brew install --cask powershell

# Script çalıştır
pwsh ./dev-setup.ps1
```

---

## 🛡️ Execution Policy

Eğer script çalıştırma hatası alırsanız:

```powershell
# Mevcut policy'yi kontrol et
Get-ExecutionPolicy

# Geçici olarak değiştir
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# Veya kalıcı olarak (admin gerekir)
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

---

## 📚 Daha Fazla Bilgi

- [Development Guide](../DEVELOPMENT_SUMMARY.md)
- [Docker Documentation](../docker-compose.yml)
- [GitHub Actions](.github/workflows/ci-cd.yml)

---

## 🤝 Katkıda Bulunma

Script iyileştirmeleri için PR gönderin:
1. Script'i geliştirin
2. Dokümantasyonu güncelleyin
3. Test edin
4. PR açın

---

## 📝 Lisans

MIT License - [LICENSE](../LICENSE)

---

**💡 İpucu:** Tüm scriptler `-?` parametresi ile help bilgisi gösterir:
```powershell
.\build-all.ps1 -?
```
