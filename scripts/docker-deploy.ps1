#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SocialMind - Docker Deployment
.DESCRIPTION
    Docker container'ları build eder, çalıştırır ve yönetir
.EXAMPLE
    .\docker-deploy.ps1 -Action Build
    .\docker-deploy.ps1 -Action Up -Environment Production
    .\docker-deploy.ps1 -Action Down
#>

param(
    [Parameter(Mandatory)]
    [ValidateSet("Build", "Up", "Down", "Restart", "Logs", "Clean", "Status")]
    [string]$Action,

    [Parameter()]
    [ValidateSet("Development", "Production")]
    [string]$Environment = "Development",

    [Parameter()]
    [switch]$Detached
)

$ErrorActionPreference = "Stop"

function Write-Success { param([string]$Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Info { param([string]$Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Warning { param([string]$Message) Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }
function Write-Section { param([string]$Message) Write-Host "`n=== $Message ===`n" -ForegroundColor Magenta }

Write-Section "🐳 SocialMind Docker Deployment"

$solutionRoot = Split-Path -Parent $PSScriptRoot

# Docker kontrolü
try {
    $dockerVersion = & docker --version 2>&1
    Write-Success "Docker: $dockerVersion"
} catch {
    Write-Error "Docker yüklü değil veya çalışmıyor!"
    Write-Info "Docker Desktop'ı indirin: https://www.docker.com/products/docker-desktop"
    exit 1
}

# Docker Compose kontrolü
try {
    $composeVersion = & docker compose version 2>&1
    Write-Success "Docker Compose: $composeVersion"
} catch {
    Write-Error "Docker Compose bulunamadı!"
    exit 1
}

# Compose dosyasını seç
$composeFile = if ($Environment -eq "Production") {
    Join-Path $solutionRoot "docker-compose.yml"
} else {
    Join-Path $solutionRoot "docker-compose.dev.yml"
}

if (-not (Test-Path $composeFile)) {
    Write-Error "Docker Compose dosyası bulunamadı: $composeFile"
    exit 1
}

Write-Info "Environment: $Environment"
Write-Info "Compose File: $composeFile"

Push-Location $solutionRoot

try {
    switch ($Action) {
        "Build" {
            Write-Section "🔨 Docker Images Build Ediliyor"

            & docker compose -f $composeFile build --no-cache

            if ($LASTEXITCODE -eq 0) {
                Write-Success "Build tamamlandı"

                # Image'leri listele
                Write-Info "`nOluşturulan images:"
                & docker images | Select-String "socialmind"
            } else {
                Write-Error "Build başarısız!"
                exit 1
            }
        }

        "Up" {
            Write-Section "🚀 Container'lar Başlatılıyor"

            $upArgs = @("-f", $composeFile, "up")

            if ($Detached) {
                $upArgs += "-d"
            }

            & docker compose @upArgs

            if ($LASTEXITCODE -eq 0) {
                Write-Success "Container'lar başlatıldı"

                Start-Sleep -Seconds 3

                Write-Info "`n📍 Erişim URL'leri:"
                Write-Host "  • Web App: http://localhost:8080" -ForegroundColor Green
                Write-Host "  • API: http://localhost:8080/api" -ForegroundColor Green

                if ($Detached) {
                    Write-Info "`nLogları görmek için: .\docker-deploy.ps1 -Action Logs"
                }
            } else {
                Write-Error "Container başlatma başarısız!"
                exit 1
            }
        }

        "Down" {
            Write-Section "🛑 Container'lar Durduruluyor"

            & docker compose -f $composeFile down

            if ($LASTEXITCODE -eq 0) {
                Write-Success "Container'lar durduruldu"
            } else {
                Write-Error "Durdurma başarısız!"
                exit 1
            }
        }

        "Restart" {
            Write-Section "🔄 Container'lar Yeniden Başlatılıyor"

            & docker compose -f $composeFile restart

            if ($LASTEXITCODE -eq 0) {
                Write-Success "Container'lar yeniden başlatıldı"
            } else {
                Write-Error "Restart başarısız!"
                exit 1
            }
        }

        "Logs" {
            Write-Section "📋 Container Logları"

            & docker compose -f $composeFile logs -f --tail=100
        }

        "Clean" {
            Write-Section "🧹 Temizlik"

            Write-Warning "Bu işlem tüm container'ları, volume'leri ve image'leri silecek!"
            $confirmation = Read-Host "Devam etmek istediğinizden emin misiniz? (y/N)"

            if ($confirmation -eq 'y') {
                # Container'ları durdur
                & docker compose -f $composeFile down -v

                # SocialMind image'lerini sil
                Write-Info "Image'ler siliniyor..."
                $images = & docker images --format "{{.Repository}}:{{.Tag}}" | Select-String "socialmind"

                foreach ($image in $images) {
                    & docker rmi $image -f
                }

                # Dangling image'leri temizle
                & docker image prune -f

                Write-Success "Temizlik tamamlandı"
            } else {
                Write-Info "İşlem iptal edildi"
            }
        }

        "Status" {
            Write-Section "📊 Container Durumu"

            # Çalışan container'lar
            Write-Host "🟢 Çalışan Container'lar:" -ForegroundColor Green
            & docker compose -f $composeFile ps

            # Image'ler
            Write-Host "`n🖼️  SocialMind Images:" -ForegroundColor Cyan
            & docker images | Select-String "socialmind" | ForEach-Object { Write-Host "  $_" }

            # Volume'ler
            Write-Host "`n💾 Volumes:" -ForegroundColor Cyan
            & docker volume ls | Select-String "socialmind" | ForEach-Object { Write-Host "  $_" }

            # Network bilgisi
            Write-Host "`n🌐 Network:" -ForegroundColor Cyan
            & docker network ls | Select-String "socialmind" | ForEach-Object { Write-Host "  $_" }

            # Resource kullanımı
            Write-Host "`n📈 Resource Kullanımı:" -ForegroundColor Magenta
            & docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}" | Select-String "socialmind"
        }
    }
} finally {
    Pop-Location
}

Write-Host "`n✨ İşlem tamamlandı!`n" -ForegroundColor Green
exit 0
