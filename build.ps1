#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Build and test script for CmdAssist PowerShell module

.DESCRIPTION
    This script builds the CmdAssist module, imports it, and provides commands to test functionality.

.PARAMETER Clean
    Clean the build output before building

.PARAMETER Import
    Import the module after building

.PARAMETER Test
    Run basic tests after building and importing

.EXAMPLE
    .\build.ps1 -Clean -Import -Test
    Cleans, builds, imports, and tests the module
#>

[CmdletBinding()]
param(
    [Switch]$Clean,
    [Switch]$Import,
    [Switch]$Test
)

# Set script location as working directory
Set-Location $PSScriptRoot

Write-Host "🚀 CmdAssist Build Script" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan

# Clean if requested
if ($Clean) {
    Write-Host "🧹 Cleaning build output..." -ForegroundColor Yellow
    if (Test-Path "src/CmdAssist.PowerShell/bin") {
        Remove-Item "src/CmdAssist.PowerShell/bin" -Recurse -Force
    }
    if (Test-Path "src/CmdAssist.PowerShell/obj") {
        Remove-Item "src/CmdAssist.PowerShell/obj" -Recurse -Force
    }
    Write-Host "✅ Clean completed" -ForegroundColor Green
}

# Build the solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
$buildResult = dotnet build --configuration Release --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Build failed!"
    exit 1
}
Write-Host "✅ Build completed successfully" -ForegroundColor Green

# Import module if requested
if ($Import) {
    Write-Host "📦 Importing CmdAssist module..." -ForegroundColor Yellow
    
    # Remove existing module if loaded
    if (Get-Module CmdAssist -ErrorAction SilentlyContinue) {
        Remove-Module CmdAssist -Force
    }
    
    $manifestPath = (Resolve-Path "src/CmdAssist.PowerShell/bin/Release/net8.0/CmdAssist.psd1").Path
    if (-not (Test-Path $manifestPath)) {
        Write-Error "❌ Module manifest not found at: $manifestPath"
        exit 1
    }
    
    try {
        Import-Module $manifestPath -Force
        Write-Host "✅ Module imported successfully" -ForegroundColor Green
        
        # Show available commands
        Write-Host "📋 Available commands:" -ForegroundColor Cyan
        Get-Command -Module CmdAssist | Format-Table Name, CommandType, Source -AutoSize
        
    } catch {
        Write-Error "❌ Failed to import module: $_"
        exit 1
    }
}

# Run tests if requested
if ($Test) {
    Write-Host "🧪 Running basic tests..." -ForegroundColor Yellow
    
    # Check if module is loaded
    if (-not (Get-Module CmdAssist -ErrorAction SilentlyContinue)) {
        Write-Error "❌ CmdAssist module is not loaded. Use -Import parameter to import it first."
        exit 1
    }
    
    # Test 1: Check if cmd-assist alias exists
    Write-Host "  Testing cmd-assist alias..." -ForegroundColor Gray
    $alias = Get-Alias -Name "cmd-assist" -ErrorAction SilentlyContinue
    if ($alias) {
        Write-Host "  ✅ cmd-assist alias found" -ForegroundColor Green
    } else {
        Write-Host "  ❌ cmd-assist alias not found" -ForegroundColor Red
    }
    
    # Test 2: Check if Invoke-CmdAssist cmdlet exists
    Write-Host "  Testing Invoke-CmdAssist cmdlet..." -ForegroundColor Gray
    $cmdlet = Get-Command -Name "Invoke-CmdAssist" -ErrorAction SilentlyContinue
    if ($cmdlet) {
        Write-Host "  ✅ Invoke-CmdAssist cmdlet found" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Invoke-CmdAssist cmdlet not found" -ForegroundColor Red
    }
    
    # Test 3: Check help
    Write-Host "  Testing help system..." -ForegroundColor Gray
    try {
        $help = Get-Help Invoke-CmdAssist -ErrorAction SilentlyContinue
        if ($help) {
            Write-Host "  ✅ Help available for Invoke-CmdAssist" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Help not available (this is normal for binary cmdlets)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  ⚠️  Help test failed: $_" -ForegroundColor Yellow
    }
    
    Write-Host "✅ Basic tests completed" -ForegroundColor Green
    
    # Show usage example
    Write-Host ""
    Write-Host "🎯 Usage Examples:" -ForegroundColor Cyan
    Write-Host "==================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "# Set up your API key (example for OpenAI):" -ForegroundColor Gray
    Write-Host '$env:OPENAI_API_KEY = "your-api-key-here"' -ForegroundColor White
    Write-Host ""
    Write-Host "# Basic usage:" -ForegroundColor Gray
    Write-Host 'cmd-assist "list all files in current directory"' -ForegroundColor White
    Write-Host ""
    Write-Host "# Execute without confirmation:" -ForegroundColor Gray
    Write-Host 'cmd-assist "show current date and time" -Confirm' -ForegroundColor White
    Write-Host ""
    Write-Host "# Use different AI provider:" -ForegroundColor Gray
    Write-Host 'cmd-assist "check disk space" -Provider Claude' -ForegroundColor White
    Write-Host ""
}

Write-Host ""
Write-Host "🎉 Script completed successfully!" -ForegroundColor Green

if (-not $Import) {
    Write-Host ""
    Write-Host "💡 Tip: Use './build.ps1 -Import' to also import the module" -ForegroundColor Cyan
}

if ($Import -and -not $Test) {
    Write-Host ""
    Write-Host "💡 Tip: Use './build.ps1 -Import -Test' to also run basic tests" -ForegroundColor Cyan
}