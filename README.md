# AudioToggle - the tool windows should of had as a default feature

AudioToggle is a W### Release Process
1. **Update Version**: Run `.\update-version.ps1 -NewVersion "1.1.0"` or manually edit `src/AudioToggle.csproj`
2. **Test Build**: Run `dotnet build src/AudioToggle.csproj` to verify
3. **Commit & Push**: `git add . && git commit -m "Bump version to 1.1.0" && git push origin main`
4. **Automatic Release**: GitHub Actions will automatically create a release with the new version

#### Manual Release (if needed)
- Go to GitHub → Releases → "Create a new release"
- Tag: `v1.0.0` (must match csproj version)
- Upload: `AudioToggle_Windows.exe` from CI artifactss utility that allows you to quickly switch between audio devices using a customizable global keyboard hotkey.

## Features

- 🎧 **Quick Device Switching**: Switch between audio devices with a single hotkey
- ⚙️ **Customizable Hotkeys**: Set your own key combinations
- ✅ **Device Selection**: Choose which devices to cycle through
- 🔄 **Live Updates**: Real-time display of current default device
- 🚀 **Windows Startup**: Run at windows startup option.

## System Requirements

- **OS**: Windows 10 version 1809 or later
- **Framework**: .NET 9.0
- **Memory**: ~50MB RAM
- **Storage**: ~200MB for installation

## Installation
Unzip release anywhere you keep your apps/utilities. Run the exe. 
In the general settings it will register/un-register itself from that setting.

#### Prerequisites
- Windows 10/11
- Administrator privileges
- .NET 9.0 Runtime


### First Time Setup
1. **Open Settings**: Right-click the system tray icon → Settings
2. **Select Devices**: Check the audio devices you want to cycle through
3. **Set Hotkey**: Click in the hotkey box and press your desired key combination
4. **Save**: Settings are automatically saved

### Default Settings
- **Default Hotkey**: Ctrl+Shift+F1
- **Enabled Devices**: All available audio devices
- **Auto-start**: Enabled when installed as service

### Settings File
Settings are stored in `settings.json` next to the executable:
```json
{
  "hotkey": "Ctrl+Shift+F1",
  "enabledDevices": ["Device1", "Device2"],
  "defaultPlayback": "CurrentDevice"
}
```

## Development

### Building from Source

#### Prerequisites
- .NET 9.0 SDK
- Windows 10/11

#### Build Commands
```bash
# Restore dependencies
dotnet restore src/AudioToggle.csproj

# Build in debug mode
dotnet build src/AudioToggle.csproj

# Build release
dotnet build src/AudioToggle.csproj -c Release

# Create Windows executable
dotnet publish src/AudioToggle.csproj -c Release -r win-x64 /p:PublishSingleFile=true /p:SelfContained=true /p:EnableCompressionInSingleFile=true -o ./dist/windows
```

#### Local Build Script
Run `build-release.bat` to build the Windows release locally.

### CI/CD Pipeline

This project uses GitHub Actions for automated building and releasing.

#### Workflow Triggers
- **Push to main**: Builds and creates artifacts
- **Pull Request**: Validates build
- **Release published**: Creates release assets

#### Build Artifacts
- `AudioToggle_Windows_v{version}.zip` - Complete Windows package containing:
  - `AudioToggle.exe` - Windows executable
  - `settings.json` - Default settings file
  - `README.txt` - Installation instructions

#### Release Process
1. Update version in `src/AudioToggle.csproj`
2. Push changes to main branch
3. GitHub Actions automatically creates release with ZIP package
4. Download `AudioToggle_Windows_v{version}.zip` from release assets

## Usage

### Basic Usage
1. Press your configured hotkey (default: Ctrl+Shift+F1)
2. Audio switches to the next enabled device in the list
3. Green checkmark shows current default device in settings

### Settings Window
- **Audio Devices Tab**: Select which devices to include in cycling
- **Hotkeys Tab**: Customize hotkey combinations
- **General Tab**: Application preferences and about information

### Hotkey Combinations
Supported modifiers: Ctrl, Alt, Shift, Win
Supported keys: F1-F12, A-Z, 0-9, Space, Enter, etc.

Examples:
- `Ctrl+Shift+F1`
- `Alt+F5`
- `Win+A`
- `Ctrl+Alt+Space`


### Hotkey Not Working
- Check if another application is using the same hotkey
- Try a different key combination


### Device Not Switching
- Verify device is enabled in settings
- Check device is connected and active
