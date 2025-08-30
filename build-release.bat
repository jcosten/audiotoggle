@echo off
echo Building AudioToggle Windows Release...
echo.

echo Restoring dependencies...
dotnet restore src/AudioToggle.csproj
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore dependencies
    exit /b 1
)

echo.
echo Building project...
dotnet build src/AudioToggle.csproj -c Release --no-restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to build project
    exit /b 1
)

echo.
echo Publishing Windows release...
dotnet publish src/AudioToggle.csproj -c Release -r win-x64 /p:PublishSingleFile=true /p:SelfContained=true /p:EnableCompressionInSingleFile=true -o ./dist/windows
if %errorlevel% neq 0 (
    echo ERROR: Failed to publish release
    exit /b 1
)

echo.
echo Creating release package with AudioToggle folder...

REM Create package directory structure
if not exist "release-package" mkdir "release-package"
if not exist "release-package\AudioToggle" mkdir "release-package\AudioToggle"

REM Copy files into AudioToggle subfolder
copy "dist\windows\AudioToggle.exe" "release-package\AudioToggle\" >nul

REM Extract version from csproj
for /f "tokens=2 delims=<>" %%i in ('findstr "<Version>" src\AudioToggle.csproj') do set VERSION=%%i

REM Create README with version
echo AudioToggle v%VERSION% > "release-package\README.txt"
echo. >> "release-package\README.txt"
echo Installation: >> "release-package\README.txt"
echo 1. Extract the ZIP file >> "release-package\README.txt"
echo 2. Open the AudioToggle folder >> "release-package\README.txt"
echo 3. Run AudioToggle.exe >> "release-package\README.txt"
echo 4. Access settings via the system tray icon >> "release-package\README.txt"
echo. >> "release-package\README.txt"
echo Package Contents: >> "release-package\README.txt"
echo - AudioToggle/AudioToggle.exe: Main application >> "release-package\README.txt"

REM Create ZIP with AudioToggle folder structure
powershell -Command "Compress-Archive -Path 'release-package\*' -DestinationPath 'AudioToggle_Windows_v%VERSION%.zip' -Force"

echo.
echo Build completed successfully!
echo Release package: AudioToggle_Windows_v%VERSION%.zip
echo.
echo Package contents:
dir /b "release-package"
echo.
echo AudioToggle folder contents:
dir /b "release-package\AudioToggle"
echo.
echo Press any key to exit...
pause > nul
