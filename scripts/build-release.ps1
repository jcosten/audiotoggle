param(
    [switch]$NoPause
)

Write-Host "Building AudioToggle Windows Release..." -ForegroundColor Green
Write-Host ""

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

Write-Host "Checking for running AudioToggle processes..." -ForegroundColor Cyan
$runningProcesses = Get-Process -Name "AudioToggle" -ErrorAction SilentlyContinue
if ($runningProcesses) {
    Write-Host "Found running AudioToggle process. Terminating..." -ForegroundColor Yellow
    $runningProcesses | Stop-Process -Force
    Start-Sleep -Seconds 2
}

Write-Host "Clearing old build artifacts..." -ForegroundColor Cyan
$pathsToClean = @("dist", "release-package")
foreach ($path in $pathsToClean) {
    if (Test-Path $path) {
        Remove-Item $path -Recurse -Force
    }
}

# Clean old ZIP files
Get-ChildItem "AudioToggle_Windows_v*.zip" -ErrorAction SilentlyContinue | Remove-Item -Force

Write-Host ""
Write-Host "Restoring dependencies..." -ForegroundColor Cyan
$restoreResult = dotnet restore src/AudioToggle.csproj
if ($LASTEXITCODE -ne 0) {
    Exit-WithError "Failed to restore dependencies"
}

Write-Host ""
Write-Host "Building project..." -ForegroundColor Cyan
$buildResult = dotnet build src/AudioToggle.csproj -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Exit-WithError "Failed to build project"
}

Write-Host ""
Write-Host "Publishing Windows release..." -ForegroundColor Cyan
$publishResult = dotnet publish src/AudioToggle.csproj -c Release -r win-x64 -o ./dist/windows --no-build
if ($LASTEXITCODE -ne 0) {
    Write-WarningMessage "Publish failed, using build output instead..."
    if (-not (Test-Path "dist\windows")) {
        New-Item -ItemType Directory -Path "dist\windows" -Force | Out-Null
    }
    Copy-Item "src\bin\Release\net9.0-windows\*" "dist\windows\" -Recurse -Force
}

Write-Host ""
Write-Host "Creating release package with AudioToggle folder..." -ForegroundColor Cyan

# Create package directory structure
$packageDir = "release-package"
$audioToggleDir = Join-Path $packageDir "AudioToggle"

if (-not (Test-Path $packageDir)) {
    New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
}
if (-not (Test-Path $audioToggleDir)) {
    New-Item -ItemType Directory -Path $audioToggleDir -Force | Out-Null
}

# Copy files into AudioToggle subfolder
Write-Host "Copying application files..." -ForegroundColor Cyan
try {
    Copy-Item "dist\windows\*" $audioToggleDir -Recurse -Force
} catch {
    Exit-WithError "Failed to copy application files"
}

# Extract version from csproj
Write-Host "Extracting version from project file..." -ForegroundColor Cyan
$version = "1.0.0" # Default version
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

# Create README with version
$readmePath = Join-Path $packageDir "README.txt"
$readmeContent = @"
AudioToggle v$version

Installation:
1. Extract the ZIP file
2. Open the AudioToggle folder
3. Run AudioToggle.exe
4. Access settings via the system tray icon

Package Contents:
- AudioToggle/AudioToggle.exe: Main application
- AudioToggle/*.dll: Required libraries
- AudioToggle/*.json: Configuration files
"@

$readmeContent | Out-File -FilePath $readmePath -Encoding UTF8

# Create ZIP with AudioToggle folder structure
Write-Host "Creating ZIP archive..." -ForegroundColor Cyan
$zipPath = "AudioToggle_Windows_v$version.zip"
try {
    Compress-Archive -Path "$packageDir\*" -DestinationPath $zipPath -Force
} catch {
    Exit-WithError "Failed to create ZIP archive"
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Release package: $zipPath" -ForegroundColor Green
Write-Host ""

Write-Host "Package contents:" -ForegroundColor Cyan
Get-ChildItem $packageDir | Format-Table -AutoSize

Write-Host ""
Write-Host "AudioToggle folder contents:" -ForegroundColor Cyan
Get-ChildItem $audioToggleDir | Format-Table -AutoSize

Write-Host ""
Write-Host "ZIP file size:" -ForegroundColor Cyan
$zipFile = Get-Item $zipPath
Write-Host ("{0:N2} MB" -f ($zipFile.Length / 1MB)) -ForegroundColor Green

Write-Host ""
if (-not $NoPause) {
    Read-Host "Press Enter to exit"
}
