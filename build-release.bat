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
echo Build completed successfully!
echo Output location: ./dist/windows/AudioToggle.exe
echo.

dir ./dist/windows
echo.
echo Press any key to exit...
pause > nul
