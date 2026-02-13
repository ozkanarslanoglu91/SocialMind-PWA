#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SocialMind - Multi-platform build script
.DESCRIPTION
    Builds Web, Windows, Android, iOS and MacCatalyst platforms
.EXAMPLE
    .\build-all.ps1
    .\build-all.ps1 -Configuration Release -Platform Web
    .\build-all.ps1 -Parallel -SkipTests
#>

param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [Parameter()]
    [ValidateSet("All", "Web", "Windows", "Android", "iOS", "MacCatalyst")]
    [string]$Platform = "All",

    [Parameter()]
    [switch]$SkipRestore,

    [Parameter()]
    [switch]$SkipClean,

    [Parameter()]
    [switch]$SkipTests,

    [Parameter()]
    [switch]$Parallel
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Output functions
function Write-Success { param([string]$Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Info { param([string]$Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Warning { param([string]$Message) Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }
function Write-Section { param([string]$Message) Write-Host "`n======================================" -ForegroundColor Magenta; Write-Host "  $Message" -ForegroundColor Magenta; Write-Host "======================================`n" -ForegroundColor Magenta }

# Start
Write-Section "SocialMind Build Automation"

$solutionRoot = Split-Path -Parent $PSScriptRoot
$solutionFile = Join-Path $solutionRoot "SocialMind.sln"

# Check solution file
if (-not (Test-Path $solutionFile)) {
    Write-Error "Solution file not found: $solutionFile"
    exit 1
}

# 1. NuGet Restore
if (-not $SkipRestore) {
    Write-Section "NuGet Package Restore"
    try {
        & dotnet restore $solutionFile
        Write-Success "NuGet restore completed"
    } catch {
        Write-Error "NuGet restore failed: $_"
        exit 1
    }
}

# 2. Clean
if (-not $SkipClean) {
    Write-Section "Cleaning Old Build Files"
    try {
        & dotnet clean $solutionFile --configuration $Configuration
        Write-Success "Clean completed"
    } catch {
        Write-Warning "Clean failed: $_"
    }
}

# 3. Build platforms
$platforms = @{
    "Web" = @{
        Project = "SocialMind.Web/SocialMind.Web.csproj"
        Framework = "net10.0"
    }
    "Windows" = @{
        Project = "SocialMind/SocialMind.csproj"
        Framework = "net10.0-windows10.0.19041.0"
    }
    "Android" = @{
        Project = "SocialMind/SocialMind.csproj"
        Framework = "net10.0-android"
    }
    "iOS" = @{
        Project = "SocialMind/SocialMind.csproj"
        Framework = "net10.0-ios"
    }
    "MacCatalyst" = @{
        Project = "SocialMind/SocialMind.csproj"
        Framework = "net10.0-maccatalyst"
    }
}

function Build-Platform {
    param(
        [string]$PlatformName,
        [hashtable]$PlatformInfo
    )

    try {
        $projectPath = Join-Path $solutionRoot $PlatformInfo.Project

        Write-Info "Building $PlatformName (${Configuration})..."

        & dotnet build $projectPath `
            --configuration $Configuration `
            --framework $PlatformInfo.Framework `
            --no-restore

        if ($LASTEXITCODE -eq 0) {
            Write-Success "$PlatformName build successful"
            return $true
        } else {
            Write-Error "$PlatformName build failed (Exit Code: $LASTEXITCODE)"
            return $false
        }
    } catch {
        Write-Error "$PlatformName build error: $_"
        return $false
    }
}

Write-Section "Building Projects"

$buildResults = @{}
$startTime = Get-Date

if ($Platform -eq "All") {
    $targetPlatforms = $platforms.Keys
} else {
    $targetPlatforms = @($Platform)
}

if ($Parallel) {
    Write-Info "Parallel build enabled"

    $jobs = $targetPlatforms | ForEach-Object {
        $platformName = $_
        $platformInfo = $platforms[$platformName]

        Start-Job -Name "Build-$platformName" -ScriptBlock {
            param($pName, $pInfo, $config, $root)
            $projectPath = Join-Path $root $pInfo.Project
            & dotnet build $projectPath --configuration $config --framework $pInfo.Framework --no-restore
            return @{ Platform = $pName; Success = ($LASTEXITCODE -eq 0) }
        } -ArgumentList $platformName, $platformInfo, $Configuration, $solutionRoot
    }

    $jobs | Wait-Job | ForEach-Object {
        $result = Receive-Job $_
        $buildResults[$result.Platform] = $result.Success
        Remove-Job $_
    }
} else {
    foreach ($platformName in $targetPlatforms) {
        $platformInfo = $platforms[$platformName]
        $success = Build-Platform -PlatformName $platformName -PlatformInfo $platformInfo
        $buildResults[$platformName] = $success
    }
}

# 4. Run tests
if (-not $SkipTests) {
    Write-Section "Running Tests"

    $testProjects = Get-ChildItem -Path $solutionRoot -Recurse -Filter "*.Tests.csproj"

    if ($testProjects.Count -eq 0) {
        Write-Warning "No test projects found"
    } else {
        foreach ($testProject in $testProjects) {
            Write-Info "Running tests: $($testProject.Name)"

            try {
                & dotnet test $testProject.FullName `
                    --configuration $Configuration `
                    --no-build `
                    --verbosity minimal

                if ($LASTEXITCODE -eq 0) {
                    Write-Success "Tests passed: $($testProject.Name)"
                } else {
                    Write-Error "Tests failed: $($testProject.Name)"
                }
            } catch {
                Write-Error "Test error: $_"
            }
        }
    }
}

# Show results
Write-Section "Build Results"

$failedBuilds = @()
foreach ($platform in $buildResults.Keys) {
    $success = $buildResults[$platform]
    if ($success) {
        Write-Success "${platform}: Build successful"
    } else {
        Write-Error "${platform}: Build failed"
        $failedBuilds += $platform
    }
}

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "`n======================================" -ForegroundColor Magenta

if ($failedBuilds.Count -eq 0) {
    Write-Success "All builds completed successfully!"
    Write-Info "Duration: $($duration.ToString('mm\:ss'))"
    Write-Host "======================================`n" -ForegroundColor Magenta
    exit 0
} else {
    Write-Error "Some builds failed!"
    Write-Info "Failed platforms: $($failedBuilds -join ', ')"
    Write-Info "Duration: $($duration.ToString('mm\:ss'))"
    Write-Host "======================================`n" -ForegroundColor Magenta
    exit 1
}
