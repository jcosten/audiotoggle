#!/usr/bin/env pwsh
# AudioToggle Test Runner Script
# Now using NUnit and Moq instead of xUnit

param(
    [switch]$Verbose,
    [switch]$Coverage
)

Write-Host "Running AudioToggle Unit Tests (NUnit + Moq)..." -ForegroundColor Green

$testCommand = "dotnet test src/AudioToggle.sln"

if ($Verbose) {
    $testCommand += " --verbosity detailed"
}

if ($Coverage) {
    $testCommand += " --collect:'XPlat Code Coverage'"
}

try {
    Invoke-Expression $testCommand
    Write-Host "`nTest execution completed successfully!" -ForegroundColor Green
} catch {
    Write-Host "`nTest execution failed!" -ForegroundColor Red
    exit 1
}
