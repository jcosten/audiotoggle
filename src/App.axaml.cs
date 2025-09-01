using System;
using System.IO;
using System.Windows.Input;
using GlobalHotKey;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.ApplicationLifetimes;

namespace AudioToggle
{
    public partial class App : Application
    {
        private AudioServiceAdapter audioService;
        
        public override void Initialize()
        {
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, "Avalonia Infrastructure");
            System.Diagnostics.Debug.WriteLine("System Diagnostics Debug");

            audioService = new AudioServiceAdapter();
            
            // Load saved hotkeys or use defaults
            RegisterSavedOutputHotkey();
            RegisterSavedInputHotkey();

            AvaloniaXamlLoader.Load(this);
        }

        private static IHotKeyService hotKeyService = new HotKeyServiceAdapter();

        public static void EnsureOutputHotkeyCallbackRegistered()
        {
            var app = Current as App;
            if (app?.audioService != null)
            {
                hotKeyService.RegisterOutputCallback(app.OnOutputHotKeyPressed);
            }
        }

        public static void EnsureInputHotkeyCallbackRegistered()
        {
            var app = Current as App;
            if (app?.audioService != null)
            {
                hotKeyService.RegisterInputCallback(app.OnInputHotKeyPressed);
            }
        }

        private void RegisterSavedOutputHotkey()
        {
            try
            {
                var savedHotkey = PersistService.GetString("outputHotkey", "Ctrl+Shift+F1");
                var (key, modifiers) = hotKeyService.ParseHotkeyString(savedHotkey);
                
                if (key.HasValue)
                {
                    var hotKey = new HotKey(key.Value, modifiers);
                    hotKeyService.RegisterOutputHotKey(hotKey);
                    hotKeyService.RegisterOutputCallback(OnOutputHotKeyPressed);
                    System.Diagnostics.Debug.WriteLine($"Registered output hotkey: {savedHotkey}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse output hotkey: {savedHotkey}");
                    // Fallback to default
                    var hotKey = new HotKey(Key.F1, ModifierKeys.Control | ModifierKeys.Shift);
                    hotKeyService.RegisterOutputHotKey(hotKey);
                    hotKeyService.RegisterOutputCallback(OnOutputHotKeyPressed);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering output hotkey: {ex.Message}");
            }
        }

        private void RegisterSavedInputHotkey()
        {
            try
            {
                var savedHotkey = PersistService.GetString("inputHotkey", "Ctrl+Shift+F2");
                var (key, modifiers) = hotKeyService.ParseHotkeyString(savedHotkey);
                
                if (key.HasValue)
                {
                    var hotKey = new HotKey(key.Value, modifiers);
                    hotKeyService.RegisterInputHotKey(hotKey);
                    hotKeyService.RegisterInputCallback(OnInputHotKeyPressed);
                    System.Diagnostics.Debug.WriteLine($"Registered input hotkey: {savedHotkey}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse input hotkey: {savedHotkey}");
                    // Fallback to default
                    var hotKey = new HotKey(Key.F2, ModifierKeys.Control | ModifierKeys.Shift);
                    hotKeyService.RegisterInputHotKey(hotKey);
                    hotKeyService.RegisterInputCallback(OnInputHotKeyPressed);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering input hotkey: {ex.Message}");
            }
        }

        private void OnHotKeyPressed()
        {
            try
            {
                var enabledDevices = audioService.GetEnabledDevicesForCycling();
                if (enabledDevices.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No enabled devices for cycling");
                    return;
                }

                var currentDefault = audioService.GetDefaultPlaybackDevice();
                int currentIndex = -1;
                
                if (currentDefault.HasValue)
                {
                    currentIndex = enabledDevices.IndexOf(currentDefault.Value.Name);
                }

                // Move to next device in the list (or first if current not found)
                int nextIndex = (currentIndex + 1) % enabledDevices.Count;
                string nextDevice = enabledDevices[nextIndex];
                
                bool success = audioService.SetDefaultPlaybackDevice(nextDevice);
                if (success)
                {
                    PersistService.StoreString("defaultPlayback", nextDevice);
                    System.Diagnostics.Debug.WriteLine($"Switched to audio device: {nextDevice}");
                    
                    // Show notification
                    NotificationService.ShowDeviceNotification(nextDevice);
                    
                    // Update the settings window UI if it exists
                    SettingsWindow.UpdateDefaultDevice(nextDevice);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to switch to audio device: {nextDevice}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in hotkey callback: {ex.Message}");
            }
        }

        private void OnOutputHotKeyPressed()
        {
            try
            {
                var enabledDevices = audioService.GetEnabledDevicesForCycling();
                if (enabledDevices.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No enabled output devices for cycling");
                    return;
                }

                var currentDefault = audioService.GetDefaultPlaybackDevice();
                int currentIndex = -1;
                
                if (currentDefault.HasValue)
                {
                    currentIndex = enabledDevices.IndexOf(currentDefault.Value.Name);
                }

                // Move to next device in the list (or first if current not found)
                int nextIndex = (currentIndex + 1) % enabledDevices.Count;
                string nextDevice = enabledDevices[nextIndex];
                
                bool success = audioService.SetDefaultPlaybackDevice(nextDevice);
                if (success)
                {
                    PersistService.StoreString("defaultPlayback", nextDevice);
                    System.Diagnostics.Debug.WriteLine($"Switched to output device: {nextDevice}");
                    
                    // Show notification
                    NotificationService.ShowDeviceNotification(nextDevice);
                    
                    // Update the settings window UI if it exists
                    SettingsWindow.UpdateDefaultDevice(nextDevice);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to switch to output device: {nextDevice}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in output hotkey callback: {ex.Message}");
            }
        }

        private void OnInputHotKeyPressed()
        {
            try
            {
                var enabledDevices = audioService.GetEnabledInputDevicesForCycling();
                if (enabledDevices.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No enabled input devices for cycling");
                    return;
                }

                var currentDefault = audioService.GetDefaultInputDevice();
                int currentIndex = -1;
                
                if (currentDefault.HasValue)
                {
                    currentIndex = enabledDevices.IndexOf(currentDefault.Value.Name);
                }

                // Move to next device in the list (or first if current not found)
                int nextIndex = (currentIndex + 1) % enabledDevices.Count;
                string nextDevice = enabledDevices[nextIndex];
                
                bool success = audioService.SetDefaultInputDevice(nextDevice);
                if (success)
                {
                    PersistService.StoreString("defaultInput", nextDevice);
                    System.Diagnostics.Debug.WriteLine($"Switched to input device: {nextDevice}");
                    
                    // Show notification
                    NotificationService.ShowDeviceNotification($"Input: {nextDevice}");
                    
                    // Update the settings window UI if it exists
                    SettingsWindow.UpdateDefaultInputDevice(nextDevice);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to switch to input device: {nextDevice}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in input hotkey callback: {ex.Message}");
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {                // For a true system tray-only application, don't create a main window
                // The tray icon and settings/notification windows will handle all UI
                desktop.MainWindow = null;
            }

            // Check if settings file exists, if not show settings window for first-time setup
            if (!PersistService.SettingsFileExists())
            {
                SettingsWindow.ShowInstance();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void OnSettings_Click(object sender, System.EventArgs args)
        {
            if (!SettingsWindow.IsInstanceVisible())
            {
                SettingsWindow.ShowInstance();
            }
        }

        public void OnExit_Click(object sender, System.EventArgs args)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }    
    }
}
