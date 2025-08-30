using System;
using System.Windows.Input;
using GlobalHotKey;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;

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
            
            // Load saved hotkey or use default
            RegisterSavedHotkey();

            AvaloniaXamlLoader.Load(this);
        }

        private static IHotKeyService hotKeyService = new HotKeyServiceAdapter();

        public static void EnsureHotkeyCallbackRegistered()
        {
            var app = Current as App;
            if (app?.audioService != null)
            {
                hotKeyService.RegisterCallback(app.OnHotKeyPressed);
            }
        }

        private void RegisterSavedHotkey()
        {
            try
            {
                var savedHotkey = PersistService.GetString("hotkey", "Ctrl+Shift+F1");
                var (key, modifiers) = hotKeyService.ParseHotkeyString(savedHotkey);
                
                if (key.HasValue)
                {
                    var hotKey = new HotKey(key.Value, modifiers);
                    hotKeyService.RegisterHotKey(hotKey);
                    hotKeyService.RegisterCallback(OnHotKeyPressed);
                    System.Diagnostics.Debug.WriteLine($"Registered hotkey: {savedHotkey}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse hotkey: {savedHotkey}");
                    // Fallback to default
                    var hotKey = new HotKey(Key.F1, ModifierKeys.Control | ModifierKeys.Alt);
                    hotKeyService.RegisterHotKey(hotKey);
                    hotKeyService.RegisterCallback(OnHotKeyPressed);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering hotkey: {ex.Message}");
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

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // For a true system tray-only application, don't create a main window
                // The tray icon and settings/notification windows will handle all UI
                desktop.MainWindow = null;
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void OnSettings_Click(object sender, System.EventArgs args)
        {
            var settingsWindow = new SettingsWindow();
            
             settingsWindow.Show();
           
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
