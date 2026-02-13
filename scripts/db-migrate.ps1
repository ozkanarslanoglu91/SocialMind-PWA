#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SocialMind - Database Migration ve Seed Management
.DESCRIPTION
    Database migration'larını yönetir, seed data ekler, backup alır
.EXAMPLE
    .\db-migrate.ps1 -Action Update
    .\db-migrate.ps1 -Action Create -MigrationName "AddNewTable"
    .\db-migrate.ps1 -Action Rollback
#>

param(
    [Parameter(Mandatory)]
    [ValidateSet("Update", "Create", "Rollback", "Backup", "Reset", "Seed", "Status")]
    [string]$Action,

    [Parameter()]
    [string]$MigrationName,

    [Parameter()]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

function Write-Success { param([string]$Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Info { param([string]$Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Warning { param([string]$Message) Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }
function Write-Section { param([string]$Message) Write-Host "`n=== $Message ===`n" -ForegroundColor Magenta }

Write-Section "🗄️  SocialMind Database Management"

$solutionRoot = Split-Path -Parent $PSScriptRoot
$webProject = Join-Path $solutionRoot "SocialMind\SocialMind.Web\SocialMind.Web.csproj"
$dbPath = Join-Path $solutionRoot "SocialMind\SocialMind.Web\socialmind.db"

# EF Core tool kontrolü
$efInstalled = & dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efInstalled) {
    Write-Warning "dotnet-ef yüklü değil. Yükleniyor..."
    & dotnet tool install --global dotnet-ef
    Write-Success "dotnet-ef yüklendi"
}

switch ($Action) {
    "Update" {
        Write-Section "📥 Database Update"
        Write-Info "Migration'lar uygulanıyor..."

        try {
            & dotnet ef database update --project $webProject --verbose
            Write-Success "Database başarıyla güncellendi"

            # Migration listesini göster
            Write-Info "Uygulanan migration'lar:"
            & dotnet ef migrations list --project $webProject --no-build
        } catch {
            Write-Error "Database update başarısız: $_"
            exit 1
        }
    }

    "Create" {
        if (-not $MigrationName) {
            Write-Error "Migration adı gerekli! Örnek: -MigrationName 'AddNewTable'"
            exit 1
        }

        Write-Section "📝 Yeni Migration Oluştur"
        Write-Info "Migration adı: $MigrationName"

        try {
            & dotnet ef migrations add $MigrationName --project $webProject
            Write-Success "Migration oluşturuldu: $MigrationName"

            # Migration dosyasını göster
            $migrationsDir = Join-Path $solutionRoot "SocialMind\SocialMind.Web\Migrations"
            $latestMigration = Get-ChildItem $migrationsDir -Filter "*$MigrationName*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

            if ($latestMigration) {
                Write-Info "Migration dosyası: $($latestMigration.FullName)"
            }
        } catch {
            Write-Error "Migration oluşturulamadı: $_"
            exit 1
        }
    }

    "Rollback" {
        Write-Section "⏮️  Database Rollback"

        # Mevcut migration'ları listele
        Write-Info "Mevcut migration'lar:"
        $migrations = & dotnet ef migrations list --project $webProject --no-build 2>&1
        Write-Host $migrations

        if (-not $Force) {
            $confirmation = Read-Host "`n⚠️  Son migration'ı geri almak istediğinizden emin misiniz? (y/N)"
            if ($confirmation -ne 'y') {
                Write-Info "İşlem iptal edildi"
                exit 0
            }
        }

        try {
            # Son migration'ı kaldır
            & dotnet ef database update 0 --project $webProject
            & dotnet ef migrations remove --project $webProject --force
            Write-Success "Rollback tamamlandı"
        } catch {
            Write-Error "Rollback başarısız: $_"
            exit 1
        }
    }

    "Backup" {
        Write-Section "💾 Database Backup"

        if (-not (Test-Path $dbPath)) {
            Write-Warning "Database dosyası bulunamadı: $dbPath"
            exit 0
        }

        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupDir = Join-Path $solutionRoot "backups"

        if (-not (Test-Path $backupDir)) {
            New-Item -ItemType Directory -Path $backupDir | Out-Null
        }

        $backupPath = Join-Path $backupDir "socialmind_$timestamp.db"

        try {
            Copy-Item $dbPath $backupPath
            Write-Success "Backup oluşturuldu: $backupPath"

            # Eski backup'ları temizle (30 günden eski)
            $oldBackups = Get-ChildItem $backupDir -Filter "socialmind_*.db" |
                Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) }

            if ($oldBackups.Count -gt 0) {
                Write-Info "30 günden eski $($oldBackups.Count) backup siliniyor..."
                $oldBackups | Remove-Item -Force
            }
        } catch {
            Write-Error "Backup başarısız: $_"
            exit 1
        }
    }

    "Reset" {
        Write-Section "🔄 Database Reset"

        if (-not $Force) {
            $confirmation = Read-Host "⚠️  TÜM DATALAR SİLİNECEK! Devam etmek istediğinizden emin misiniz? (yes/no)"
            if ($confirmation -ne 'yes') {
                Write-Info "İşlem iptal edildi"
                exit 0
            }
        }

        try {
            # Backup al
            if (Test-Path $dbPath) {
                Write-Info "Önce backup alınıyor..."
                & $PSScriptRoot\db-migrate.ps1 -Action Backup
            }

            # Database'i sil
            if (Test-Path $dbPath) {
                Remove-Item $dbPath -Force
                Write-Success "Eski database silindi"
            }

            # Yeni database oluştur
            & dotnet ef database update --project $webProject
            Write-Success "Yeni database oluşturuldu"

            # Seed data ekle
            Write-Info "Seed data ekleniyor..."
            & $PSScriptRoot\db-migrate.ps1 -Action Seed -Force

        } catch {
            Write-Error "Reset başarısız: $_"
            exit 1
        }
    }

    "Seed" {
        Write-Section "🌱 Seed Data"
        Write-Info "Seed data ekleniyor..."

        # Not: Seed data Program.cs'de tanımlı (SeedAdminData)
        # Web app'i başlatarak otomatik seed yapılır

        Write-Info "Web aplikasyonu başlatılıyor (seed için)..."
        try {
            $webProjectDir = Split-Path $webProject
            Push-Location $webProjectDir

            # Kısa süre çalıştır ve durdur
            $process = Start-Process -FilePath "dotnet" -ArgumentList "run --no-build" -PassThru -NoNewWindow
            Start-Sleep -Seconds 10
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue

            Pop-Location
            Write-Success "Seed data eklendi"

            Write-Info @"

Default Admin Credentials:
━━━━━━━━━━━━━━━━━━━━━━━━
Email: admin@socialmind.com
Password: Admin123!
━━━━━━━━━━━━━━━━━━━━━━━━

"@
        } catch {
            Pop-Location
            Write-Warning "Seed işlemi tamamlanamadı: $_"
        }
    }

    "Status" {
        Write-Section "📊 Database Status"

        # Database dosyası kontrolü
        if (Test-Path $dbPath) {
            $dbInfo = Get-Item $dbPath
            Write-Success "Database mevcut"
            Write-Info "  Path: $($dbInfo.FullName)"
            Write-Info "  Size: $([math]::Round($dbInfo.Length / 1MB, 2)) MB"
            Write-Info "  Modified: $($dbInfo.LastWriteTime)"
        } else {
            Write-Warning "Database dosyası bulunamadı"
        }

        # Migration'ları listele
        Write-Host "`n📋 Migration'lar:" -ForegroundColor Cyan
        try {
            & dotnet ef migrations list --project $webProject --no-build
        } catch {
            Write-Warning "Migration listesi alınamadı"
        }

        # Backup'ları listele
        $backupDir = Join-Path $solutionRoot "backups"
        if (Test-Path $backupDir) {
            $backups = Get-ChildItem $backupDir -Filter "socialmind_*.db" | Sort-Object LastWriteTime -Descending

            if ($backups.Count -gt 0) {
                Write-Host "`n💾 Backup'lar:" -ForegroundColor Cyan
                foreach ($backup in $backups | Select-Object -First 5) {
                    $size = [math]::Round($backup.Length / 1MB, 2)
                    Write-Info "  $($backup.Name) - ${size}MB - $($backup.LastWriteTime)"
                }

                if ($backups.Count -gt 5) {
                    Write-Info "  ... ve $($backups.Count - 5) tane daha"
                }
            }
        }
    }
}

Write-Host "`n✨ İşlem tamamlandı!`n" -ForegroundColor Green
exit 0
