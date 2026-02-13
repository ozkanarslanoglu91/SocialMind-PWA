#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SocialMind - Test Automation
.DESCRIPTION
    Unit tests, integration tests ve kod coverage testlerini çalıştırır
.EXAMPLE
    .\run-tests.ps1 -Type All
    .\run-tests.ps1 -Type Unit -Coverage
    .\run-tests.ps1 -Type Integration -Verbose
#>

param(
    [Parameter()]
    [ValidateSet("All", "Unit", "Integration", "E2E", "Performance")]
    [string]$Type = "All",

    [Parameter()]
    [switch]$Coverage,

    [Parameter()]
    [switch]$Watch,

    [Parameter()]
    [switch]$Verbose,

    [Parameter()]
    [string]$Filter
)

$ErrorActionPreference = "Stop"

function Write-Success { param([string]$Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Info { param([string]$Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Warning { param([string]$Message) Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }
function Write-Section { param([string]$Message) Write-Host "`n=== $Message ===`n" -ForegroundColor Magenta }

Write-Section "🧪 SocialMind Test Automation"

$solutionRoot = Split-Path -Parent $PSScriptRoot
$solutionFile = Join-Path $solutionRoot "SocialMind.sln"

# Test projelerini bul
$testProjects = Get-ChildItem -Path $solutionRoot -Recurse -Filter "*.Tests.csproj"

if ($testProjects.Count -eq 0) {
    Write-Warning "Test projesi bulunamadı!"
    Write-Info "Test projesi oluşturmak için:"
    Write-Host "  dotnet new xunit -n SocialMind.Tests -o tests/SocialMind.Tests" -ForegroundColor Yellow
    exit 0
}

Write-Info "Bulunan test projeleri:"
foreach ($project in $testProjects) {
    Write-Host "  • $($project.Name)" -ForegroundColor Cyan
}

# Test argümanlarını hazırla
$testArgs = @(
    "test"
    $solutionFile
    "--configuration", "Debug"
    "--logger", "console;verbosity=normal"
    "--logger", "trx"
    "--logger", "html"
)

if ($Verbose) {
    $testArgs += "--verbosity", "detailed"
} else {
    $testArgs += "--verbosity", "minimal"
}

if ($Coverage) {
    $testArgs += @(
        "--collect", "XPlat Code Coverage"
        "--results-directory", "./TestResults"
    )

    Write-Info "Code coverage etkin"
}

if ($Watch) {
    $testArgs += "watch"
    Write-Info "Watch mode etkin (değişiklikleri izliyor)"
}

if ($Filter) {
    $testArgs += "--filter", $Filter
    Write-Info "Filter: $Filter"
}

# Test tipine göre filtrele
$categoryFilter = switch ($Type) {
    "Unit" { "Category=Unit" }
    "Integration" { "Category=Integration" }
    "E2E" { "Category=E2E" }
    "Performance" { "Category=Performance" }
    default { $null }
}

if ($categoryFilter) {
    if ($Filter) {
        $testArgs += "&", $categoryFilter
    } else {
        $testArgs += "--filter", $categoryFilter
    }
    Write-Info "Test tipi: $Type"
}

# Testleri çalıştır
Write-Section "🚀 Testler Çalıştırılıyor"

$startTime = Get-Date

try {
    & dotnet @testArgs
    $exitCode = $LASTEXITCODE

    $endTime = Get-Date
    $duration = $endTime - $startTime

    if ($exitCode -eq 0) {
        Write-Section "✨ Test Sonuçları"
        Write-Success "Tüm testler başarılı!"
        Write-Info "Süre: $($duration.ToString('mm\:ss'))"

        # Test sonuç dosyalarını listele
        $testResults = Get-ChildItem -Path $solutionRoot -Recurse -Filter "*.trx" |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 5

        if ($testResults.Count -gt 0) {
            Write-Info "`nTest sonuç dosyaları:"
            foreach ($result in $testResults) {
                Write-Host "  📄 $($result.FullName)" -ForegroundColor Gray
            }
        }

    } else {
        Write-Section "❌ Test Sonuçları"
        Write-Error "Bazı testler başarısız!"
        Write-Info "Süre: $($duration.ToString('mm\:ss'))"
    }

    # Coverage raporu
    if ($Coverage) {
        Write-Section "📊 Code Coverage"

        $coverageFiles = Get-ChildItem -Path "./TestResults" -Recurse -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue

        if ($coverageFiles.Count -gt 0) {
            Write-Success "Coverage raporu oluşturuldu"

            # ReportGenerator kurulu mu?
            $reportGeneratorInstalled = & dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"

            if (-not $reportGeneratorInstalled) {
                Write-Warning "ReportGenerator yüklü değil"
                Write-Info "Yüklemek için: dotnet tool install -g dotnet-reportgenerator-globaltool"
            } else {
                # HTML rapor oluştur
                $reportDir = "./TestResults/CoverageReport"

                Write-Info "HTML rapor oluşturuluyor..."
                & reportgenerator `
                    -reports:"./TestResults/**/coverage.cobertura.xml" `
                    -targetdir:$reportDir `
                    -reporttypes:"Html;Badges"

                $reportIndex = Join-Path $reportDir "index.html"
                if (Test-Path $reportIndex) {
                    Write-Success "Rapor oluşturuldu: $reportIndex"
                    Write-Info "Raporu açmak için: start $reportIndex"
                }
            }

            # Coverage dosyasını göster
            Write-Info "`nCoverage dosyaları:"
            foreach ($coverage in $coverageFiles | Select-Object -First 1) {
                Write-Host "  📊 $($coverage.FullName)" -ForegroundColor Gray
            }
        } else {
            Write-Warning "Coverage dosyası bulunamadı"
        }
    }

    exit $exitCode

} catch {
    Write-Error "Test çalıştırma hatası: $_"
    exit 1
}
