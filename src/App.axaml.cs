using System;
using System.Windows.Input;
using GlobalHotKey;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;

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

        public static void EnsureHotkeyCallbackRegistered()
        {
            // This method can be called to ensure the hotkey callback is registered
            // Useful when the hotkey is changed from the settings UI
            var app = Current as App;
            if (app?.audioService != null)
            {
                HotKeyService.RegisterCallback(app.OnHotKeyPressed);
            }
        }

        private void RegisterSavedHotkey()
        {
            try
            {
                var savedHotkey = PersistService.GetString("hotkey", "Ctrl+Shift+F1");
                var (key, modifiers) = ParseHotkeyString(savedHotkey);
                
                if (key.HasValue)
                {
                    var hotKey = new HotKey(key.Value, modifiers);
                    HotKeyService.RegisterHotKey(hotKey);
                    HotKeyService.RegisterCallback(OnHotKeyPressed);
                    System.Diagnostics.Debug.WriteLine($"Registered hotkey: {savedHotkey}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse hotkey: {savedHotkey}");
                    // Fallback to default
                    var hotKey = new HotKey(Key.F5, ModifierKeys.Control | ModifierKeys.Alt);
                    HotKeyService.RegisterHotKey(hotKey);
                    HotKeyService.RegisterCallback(OnHotKeyPressed);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering hotkey: {ex.Message}");
            }
        }

        private (Key?, ModifierKeys) ParseHotkeyString(string hotkeyString)
        {
            if (string.IsNullOrEmpty(hotkeyString))
                return (null, ModifierKeys.None);

            var parts = hotkeyString.Split('+');
            if (parts.Length == 0)
                return (null, ModifierKeys.None);

            ModifierKeys modifiers = ModifierKeys.None;
            Key? mainKey = null;

            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                switch (trimmedPart.ToLower())
                {
                    case "ctrl":
                        modifiers |= ModifierKeys.Control;
                        break;
                    case "alt":
                        modifiers |= ModifierKeys.Alt;
                        break;
                    case "shift":
                        modifiers |= ModifierKeys.Shift;
                        break;
                    case "win":
                        modifiers |= ModifierKeys.Windows;
                        break;
                    default:
                        mainKey = ParseKeyString(trimmedPart);
                        break;
                }
            }

            return (mainKey, modifiers);
        }

        private Key? ParseKeyString(string keyString)
        {
            switch (keyString.ToUpper())
            {
                case "F1": return Key.F1;
                case "F2": return Key.F2;
                case "F3": return Key.F3;
                case "F4": return Key.F4;
                case "F5": return Key.F5;
                case "F6": return Key.F6;
                case "F7": return Key.F7;
                case "F8": return Key.F8;
                case "F9": return Key.F9;
                case "F10": return Key.F10;
                case "F11": return Key.F11;
                case "F12": return Key.F12;
                case "A": return Key.A;
                case "B": return Key.B;
                case "C": return Key.C;
                case "D": return Key.D;
                case "E": return Key.E;
                case "F": return Key.F;
                case "G": return Key.G;
                case "H": return Key.H;
                case "I": return Key.I;
                case "J": return Key.J;
                case "K": return Key.K;
                case "L": return Key.L;
                case "M": return Key.M;
                case "N": return Key.N;
                case "O": return Key.O;
                case "P": return Key.P;
                case "Q": return Key.Q;
                case "R": return Key.R;
                case "S": return Key.S;
                case "T": return Key.T;
                case "U": return Key.U;
                case "V": return Key.V;
                case "W": return Key.W;
                case "X": return Key.X;
                case "Y": return Key.Y;
                case "Z": return Key.Z;
                case "0":
                case "D0": return Key.D0;
                case "1":
                case "D1": return Key.D1;
                case "2":
                case "D2": return Key.D2;
                case "3":
                case "D3": return Key.D3;
                case "4":
                case "D4": return Key.D4;
                case "5":
                case "D5": return Key.D5;
                case "6":
                case "D6": return Key.D6;
                case "7":
                case "D7": return Key.D7;
                case "8":
                case "D8": return Key.D8;
                case "9":
                case "D9": return Key.D9;
                case "SPACE": return Key.Space;
                case "ENTER": return Key.Enter;
                case "ESCAPE": return Key.Escape;
                case "TAB": return Key.Tab;
                case "BACK": return Key.Back;
                case "DELETE": return Key.Delete;
                case "INSERT": return Key.Insert;
                case "HOME": return Key.Home;
                case "END": return Key.End;
                case "PAGEUP": return Key.PageUp;
                case "PAGEDOWN": return Key.PageDown;
                case "UP": return Key.Up;
                case "DOWN": return Key.Down;
                case "LEFT": return Key.Left;
                case "RIGHT": return Key.Right;
                default: return null;
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
            // Do not create or assign MainWindow; keep it always hidden
            base.OnFrameworkInitializationCompleted();
        }

        public void OnSettings_Click(object sender, System.EventArgs args)
        {


            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Show();
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
