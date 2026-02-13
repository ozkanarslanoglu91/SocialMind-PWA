#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SocialMind - Development Environment Setup
.DESCRIPTION
    Geliştirme ortamını otomatik olarak hazırlar (dependencies, database, secrets)
#>

$ErrorActionPreference = "Stop"

function Write-Success { param([string]$Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Info { param([string]$Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Warning { param([string]$Message) Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }
function Write-Section { param([string]$Message) Write-Host "`n=== $Message ===`n" -ForegroundColor Magenta }

Write-Section "🚀 SocialMind Development Setup"

$solutionRoot = Split-Path -Parent $PSScriptRoot

# 1. Araç kontrolü
Write-Section "🔍 Araç Kontrolü"

$tools = @(
    @{Name=".NET SDK"; Command="dotnet"; Version="--version"},
    @{Name="Git"; Command="git"; Version="--version"},
    @{Name="Node.js"; Command="node"; Version="--version"},
    @{Name="NPM"; Command="npm"; Version="--version"}
)

foreach ($tool in $tools) {
    try {
        $version = & $tool.Command $tool.Version 2>&1 | Select-Object -First 1
        Write-Success "$($tool.Name): $version"
    } catch {
        Write-Error "$($tool.Name) bulunamadı! Lütfen yükleyin."
        exit 1
    }
}

# 2. .NET Workloads
Write-Section "📦 .NET MAUI Workload Kontrolü"
try {
    $workloads = & dotnet workload list 2>&1

    if ($workloads -match "maui") {
        Write-Success "MAUI workload yüklü"
    } else {
        Write-Warning "MAUI workload yüklü değil. Yükleniyor..."
        & dotnet workload install maui
        Write-Success "MAUI workload yüklendi"
    }
} catch {
    Write-Error "Workload kontrolü başarısız: $_"
}

# 3. NuGet Restore
Write-Section "📦 NuGet Paketleri"
try {
    & dotnet restore "$solutionRoot\SocialMind.sln"
    Write-Success "NuGet paketleri restore edildi"
} catch {
    Write-Error "NuGet restore başarısız: $_"
    exit 1
}

# 4. Node Dependencies
Write-Section "📦 Node Dependencies"
if (Test-Path "$solutionRoot\package.json") {
    try {
        Push-Location $solutionRoot
        & npm install
        Write-Success "Node modülleri yüklendi"
        Pop-Location
    } catch {
        Write-Warning "NPM install başarısız: $_"
    }
}

# 5. Environment Variables
Write-Section "🔐 Environment Variables"
$envExample = Join-Path $solutionRoot ".env.example"
$envFile = Join-Path $solutionRoot ".env"

if (Test-Path $envExample) {
    if (-not (Test-Path $envFile)) {
        Copy-Item $envExample $envFile
        Write-Success ".env dosyası oluşturuldu (lütfen API keylerini girin)"
    } else {
        Write-Info ".env dosyası zaten mevcut"
    }
} else {
    Write-Warning ".env.example dosyası bulunamadı"
}

# 6. appsettings.Development.json
Write-Section "⚙️  Development Settings"
$webProjectFolder = Join-Path $solutionRoot "SocialMind\SocialMind.Web"
$devSettings = Join-Path $webProjectFolder "appsettings.Development.json"

if (-not (Test-Path $devSettings)) {
    $devSettingsContent = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "DetailedErrors": true,
  "AppSettings": {
    "EnableMockServices": true
  }
}
"@
    Set-Content -Path $devSettings -Value $devSettingsContent
    Write-Success "appsettings.Development.json oluşturuldu"
} else {
    Write-Info "appsettings.Development.json zaten mevcut"
}

# 7. Database
Write-Section "🗄️  Database Migration"
$webProjectPath = Join-Path $solutionRoot "SocialMind\SocialMind.Web\SocialMind.Web.csproj"

try {
    Write-Info "Entity Framework durumu kontrol ediliyor..."

    # EF Tool global olarak yüklü mü?
    $efInstalled = & dotnet tool list -g | Select-String "dotnet-ef"

    if (-not $efInstalled) {
        Write-Warning "dotnet-ef global tool yüklü değil. Yükleniyor..."
        & dotnet tool install --global dotnet-ef
    }

    # Migration'ları uygula
    Write-Info "Database migration uygulanıyor..."
    & dotnet ef database update --project $webProjectPath
    Write-Success "Database migration tamamlandı"
} catch {
    Write-Warning "Database migration hatası: $_"
    Write-Info "Manuel olarak çalıştırın: cd SocialMind.Web && dotnet ef database update"
}

# 8. Git Hooks
Write-Section "🪝 Git Hooks"
$gitHooksDir = Join-Path $solutionRoot ".git\hooks"
if (Test-Path $gitHooksDir) {
    # Pre-commit hook
    $preCommitHook = Join-Path $gitHooksDir "pre-commit"
    $preCommitContent = @"
#!/bin/sh
# SocialMind Pre-commit Hook
echo "🔍 Running pre-commit checks..."

# Format check
dotnet format --verify-no-changes --verbosity quiet
if [ $? -ne 0 ]; then
    echo "❌ Code formatting issues found. Run 'dotnet format' to fix."
    exit 1
fi

echo "✅ Pre-commit checks passed"
exit 0
"@
    Set-Content -Path $preCommitHook -Value $preCommitContent -NoNewline
    Write-Success "Git pre-commit hook oluşturuldu"
} else {
    Write-Warning "Git hooks dizini bulunamadı"
}

# 9. VS Code Extensions
Write-Section "📝 VS Code Extensions Önerisi"
$extensionsFile = Join-Path $solutionRoot ".vscode\extensions.json"

if (-not (Test-Path (Split-Path $extensionsFile))) {
    New-Item -ItemType Directory -Path (Split-Path $extensionsFile) -Force | Out-Null
}

if (-not (Test-Path $extensionsFile)) {
    $extensions = @"
{
  "recommendations": [
    "ms-dotnettools.csdevkit",
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscode-dotnet-runtime",
    "ms-vscode.powershell",
    "esbenp.prettier-vscode",
    "dbaeumer.vscode-eslint",
    "GitHub.copilot",
    "GitHub.copilot-chat",
    "eamodio.gitlens",
    "ms-azuretools.vscode-docker"
  ]
}
"@
    Set-Content -Path $extensionsFile -Value $extensions
    Write-Success "VS Code extensions.json oluşturuldu"
}

# Özet
Write-Section "✨ Setup Tamamlandı!"
Write-Host @"

📌 Sonraki Adımlar:

1. 🔑 API Keylerini yapılandırın:
   • .env dosyasını düzenleyin
   • appsettings.json'da Stripe, OAuth keys'leri ekleyin

2. 🚀 Uygulamayı çalıştırın:
   • Web: cd SocialMind.Web && dotnet run
   • Windows: cd SocialMind && dotnet run -f net10.0-windows10.0.19041.0

3. 📖 Dokümantasyonu okuyun:
   • README.md
   • DEVELOPMENT_SUMMARY.md

🎉 Happy coding!

"@ -ForegroundColor Cyan

exit 0
