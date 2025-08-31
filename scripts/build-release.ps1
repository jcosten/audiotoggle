param(
    [switch]$NoPause
)

Write-Host "Building AudioToggle Windows Release..." -ForegroundColor Green

# Function to handle errors
function Exit-WithError {
    param([string]$Message)
    Write-Host "ERROR: $Message" -ForegroundColor Red
    if (-not $NoPause) {
        Read-Host "Press Enter to exit"
    }
    exit 1
}

# Function to handle warnings
function Write-WarningMessage {
    param([string]$Message)
    Write-Host "WARNING: $Message" -ForegroundColor Yellow
}

# Clean dist folder
Write-Host "Cleaning dist folder..." -ForegroundColor Cyan
if (Test-Path "dist") {
    try {
        Remove-Item "dist" -Recurse -Force
    } catch {
        Write-WarningMessage "Could not clean dist folder completely, some files may be in use"
        # Try to remove individual files
        Get-ChildItem "dist" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue
    }
}

# Clean old ZIP files in root (for backward compatibility)
Get-ChildItem "AudioToggle_Windows_v*.zip" -ErrorAction SilentlyContinue | Remove-Item -Force

# Extract version from csproj
Write-Host "Extracting version from project file..." -ForegroundColor Cyan
$version = "1.0.0"
try {
    $csprojPath = "src\AudioToggle.csproj"
    if (Test-Path $csprojPath) {
        $xml = [xml](Get-Content $csprojPath)
        $versionNode = $xml.Project.PropertyGroup.Version
        if ($versionNode) {
            $version = $versionNode
        }
    }
} catch {
    Write-WarningMessage "Could not extract version from csproj file, using default"
}

Write-Host "Using version: $version" -ForegroundColor Green

# Restore and build
Write-Host "Restoring dependencies..." -ForegroundColor Cyan
dotnet restore src/AudioToggle.csproj
if ($LASTEXITCODE -ne 0) {
    Exit-WithError "Failed to restore dependencies"
}

Write-Host "Building and publishing..." -ForegroundColor Cyan
dotnet publish src/AudioToggle.csproj -c Release -r win-x64 -o ./dist/windows --no-restore
if ($LASTEXITCODE -ne 0) {
    Exit-WithError "Failed to build and publish"
}

# Create ZIP from dist folder
Write-Host "Creating ZIP archive..." -ForegroundColor Cyan
$zipPath = "dist\AudioToggle_Windows_v$version.zip"
Compress-Archive -Path "dist\windows\*" -DestinationPath $zipPath -Force

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Release package: $zipPath" -ForegroundColor Green

$zipFile = Get-Item $zipPath
Write-Host ("ZIP file size: {0:N2} MB" -f ($zipFile.Length / 1MB)) -ForegroundColor Green

if (-not $NoPause) {
    Read-Host "Press Enter to exit"
}
