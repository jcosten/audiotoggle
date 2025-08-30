# AudioToggle - the tool windows should of had as a default feature

AudioToggle is a Windows utility that allows you to quickly switch between audio devices using a customizable global keyboard hotkey.

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

### Option 1: Run as Regular Application
1. Build the application:
   ```bash
   dotnet build -c Release
   ```
2. Run the executable:
   ```bash
   dotnet run
   ```

### Option 2: Install as Windows Service (Recommended)

#### Prerequisites
- Windows 10/11
- Administrator privileges
- .NET 9.0 Runtime

#### Installation Steps

1. **Build the Release Version**:
   ```bash
   dotnet publish -c Release
   ```

2. **Run the Installation Script**:
   - **Method A**: Double-click `install-service.bat` and select "Run as administrator"
   - **Method B**: Open PowerShell as Administrator and run:
     ```powershell
     .\install-service.ps1
     ```

3. **Verify Installation**:
   - Open Services (services.msc)
   - Look for "AudioToggle - Audio Device Switcher"
   - Status should be "Running"


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
- Restart the service: `net stop AudioToggleService && net start AudioToggleService`


### Device Not Switching
- Verify device is enabled in settings
- Check device is connected and active
