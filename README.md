# AudioToggle - One keyboard hotkey to toggle between audio devices

AudioToggle is a Windows utility for switching between audio devices using customizable global hotkeys.

## Features

- 🎧 **Quick Device Switching**: Switch between audio devices with a single hotkey
- ⚙️ **Customizable Hotkeys**: Set your own key combination
- ✅ **Device Selection**: Choose which devices to cycle through
- 🔄 **Switch Notification**: Notification of what the default is when changed via the hotkey
- 🚀 **Windows Startup**: Run at windows startup option.

## System Requirements

- **OS**: Windows 10 version 1809 or later
- **Memory**: ~100MB RAM (yes this is too much, future improvement)
- **Storage**: ~85MB for installation

## Installation

Unzip release anywhere you keep your apps/utilities. Run the exe.

### First Time Setup
2. **Select Devices**: Check the audio devices you want to cycle through
3. **Set Hotkey**: Click in the hotkey box and press your desired key combination **Default Hotkey**: Ctrl+F1
4. **Save**: Settings are automatically saved

### Basic Usage
1. Press your configured hotkey (default: Ctrl+F1)
2. Audio switches to the next enabled device in the list
3. Green checkmark shows current default device in settings

### Settings Window
- **Audio Devices Tab**: Select which devices to include in cycling
- **Hotkeys Tab**: Customize hotkey combinations
- **General Tab**: Application preferences and about information

### Hotkey Combinations
Supported modifiers: Ctrl, Alt, Shift, Win
Supported keys: F1-F12, A-Z, 0-9, Space, Enter, etc.

## Development







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

# Build release and package for windows
scripts/build-release.ps1
```

#### Release Process
1. Update version in `src/AudioToggle.csproj`
2. Push changes to main branch
3. GitHub Actions automatically creates release with ZIP package
4. Download `AudioToggle_Windows_v{version}.zip` from release assets
