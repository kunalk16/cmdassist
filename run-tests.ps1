# Test runner script for CmdAssist.PowerShell.Tests

param(
    [switch]$Coverage,
    [switch]$Verbose,
    [string]$Filter = ""
)

Write-Host "Running CmdAssist.PowerShell Tests..." -ForegroundColor Green

$testArgs = @("test")

if ($Coverage) {
    $testArgs += "--collect:XPlat Code Coverage"
    Write-Host "Code coverage enabled" -ForegroundColor Yellow
}

if ($Verbose) {
    $testArgs += "--verbosity", "detailed"
}

if ($Filter) {
    $testArgs += "--filter", $Filter
    Write-Host "Filter applied: $Filter" -ForegroundColor Yellow
}

# Change to test project directory
$testPath = Join-Path $PSScriptRoot "tests\CmdAssist.PowerShell.Tests"
Push-Location $testPath

try {
    # Run tests
    & dotnet @testArgs
    
    if ($Coverage) {
        Write-Host "`nLooking for coverage reports..." -ForegroundColor Yellow
        $coverageFiles = Get-ChildItem -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1
        if ($coverageFiles) {
            Write-Host "Coverage report generated: $($coverageFiles.FullName)" -ForegroundColor Green
        }
    }
}
finally {
    Pop-Location
}

Write-Host "`nTest run completed!" -ForegroundColor Green