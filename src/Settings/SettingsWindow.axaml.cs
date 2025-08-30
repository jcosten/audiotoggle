using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using GlobalHotKey;

namespace AudioToggle
{

    public partial class SettingsWindow : Window
    {
        private readonly AudioServiceAdapter audioServiceAdapter;
        private List<DeviceViewModel> deviceViewModels;
        private static SettingsWindow _instance;

        public SettingsWindow()
        {
            _instance = this;
            this.audioServiceAdapter = new AudioServiceAdapter();
            AvaloniaXamlLoader.Load(this);

            InitializeAudioDevicesTab();
            InitializeHotkeysTab();
            InitializeGeneralTab();

            this.Closing += (sender, e) => {
                e.Cancel = true;
                this.Hide();
            };
        }

        // Static method to update the default device from external classes
        public static void UpdateDefaultDevice(string newDefaultDeviceName)
        {
            _instance?.RefreshDefaultDevice(newDefaultDeviceName);
        }

        private void RefreshDefaultDevice(string newDefaultDeviceName)
        {
            if (deviceViewModels != null)
            {
                // Update the IsDefault property for all devices
                foreach (var device in deviceViewModels)
                {
                    device.IsDefault = device.Name == newDefaultDeviceName;
                }
            }
        }

        private void InitializeAudioDevicesTab()
        {
            var listBox = this.FindControl<ListBox>("AudioDevicesListBox");

            // Invalidate cache to ensure we get fresh device information
            this.audioServiceAdapter.InvalidateCache();
            
            var deviceNames = this.audioServiceAdapter.GetAudioDeviceNames();
            var defaultDevice = GetDefaultDeviceName();
            var enabledDevices = GetEnabledDevices();
            deviceViewModels = new List<DeviceViewModel>();
            
            foreach (var name in deviceNames)
            {
                var deviceViewModel = new DeviceViewModel
                {
                    Name = name,
                    IsDefault = name == defaultDevice,
                    IsEnabled = enabledDevices.Contains(name)
                };
                
                // Subscribe to changes in the IsEnabled property
                deviceViewModel.EnabledChanged += (s, e) => SaveEnabledDevices(deviceViewModels);
                deviceViewModels.Add(deviceViewModel);
            }
            
            listBox.ItemsSource = deviceViewModels;
        }

        private List<string> GetEnabledDevices()
        {
            var enabledDevicesJson = PersistService.GetString("enabledDevices", "[]");
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(enabledDevicesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void SaveEnabledDevices(List<DeviceViewModel> devices)
        {
            var enabledDevices = devices.Where(d => d.IsEnabled).Select(d => d.Name).ToList();
            var json = System.Text.Json.JsonSerializer.Serialize(enabledDevices);
            PersistService.StoreString("enabledDevices", json);
        }

        private void InitializeHotkeysTab()
        {
            var changeHotkeyButton = this.FindControl<Button>("ChangeHotkeyButton");
            var hotkeyTextBox = this.FindControl<TextBox>("HotkeyTextBox");

            // Load saved hotkey or use default
            var savedHotkey = PersistService.GetString("hotkey", "Ctrl+Alt+F5");
            hotkeyTextBox.Text = savedHotkey;

            // Handle key input for hotkey capture
            hotkeyTextBox.KeyDown += OnHotkeyTextBoxKeyDown;
            hotkeyTextBox.GotFocus += (s, e) => 
            {
                hotkeyTextBox.Text = "Press key combination...";
                hotkeyTextBox.SelectAll();
            };
            hotkeyTextBox.LostFocus += (s, e) =>
            {
                if (hotkeyTextBox.Text == "Press key combination...")
                {
                    hotkeyTextBox.Text = savedHotkey;
                }
            };

            changeHotkeyButton.Click += OnResetHotkeyClicked;
        }

        private void InitializeGeneralTab()
        {
            var startWithWindowsCheckBox = this.FindControl<CheckBox>("StartWithWindowsCheckBox");
            var showNotificationsCheckBox = this.FindControl<CheckBox>("ShowNotificationsCheckBox");

            // Load saved settings and check actual startup status
            var isStartupEnabled = WindowsStartupManager.IsStartupEnabled();
            startWithWindowsCheckBox.IsChecked = isStartupEnabled;
            showNotificationsCheckBox.IsChecked = PersistService.GetBool("showNotifications", true);

            // Handle setting changes
            startWithWindowsCheckBox.Click += (s, e) =>
            {
                var isChecked = startWithWindowsCheckBox.IsChecked ?? false;
                var success = WindowsStartupManager.SetStartupEnabled(isChecked);
                
                if (success)
                {
                    PersistService.StoreBool("startWithWindows", isChecked);
                    System.Diagnostics.Debug.WriteLine($"Startup setting changed to: {isChecked}");
                }
                else
                {
                    // Revert checkbox if operation failed
                    startWithWindowsCheckBox.IsChecked = !isChecked;
                    ShowStartupError("Failed to modify Windows startup setting. Please check your permissions.");
                }
            };
            showNotificationsCheckBox.Click += (s, e) => PersistService.StoreBool("showNotifications", showNotificationsCheckBox.IsChecked ?? false);
        }

        private void ShowStartupError(string message)
        {
            var errorButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            
            var errorWindow = new Window
            {
                Title = "Startup Setting Error",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Windows Startup Configuration",
                            FontWeight = Avalonia.Media.FontWeight.Bold,
                            Margin = new Avalonia.Thickness(0, 0, 0, 10)
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Avalonia.Thickness(0, 0, 0, 10)
                        },
                        errorButton
                    }
                }
            };
            
            errorButton.Click += (s, e) => errorWindow.Close();
            errorWindow.ShowDialog(this);
        }

        private void OnHotkeyTextBoxKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            e.Handled = true;
            
            var hotkeyTextBox = sender as TextBox;
            if (hotkeyTextBox == null) return;

            // Build modifier string
            var modifiers = new List<string>();
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                modifiers.Add("Ctrl");
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
                modifiers.Add("Alt");
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                modifiers.Add("Shift");
            if (e.KeyModifiers.HasFlag(KeyModifiers.Meta))
                modifiers.Add("Win");

            // Get the main key (ignore modifier keys themselves)
            string mainKey = null;
            switch (e.Key)
            {
                case Avalonia.Input.Key.LeftCtrl:
                case Avalonia.Input.Key.RightCtrl:
                case Avalonia.Input.Key.LeftAlt:
                case Avalonia.Input.Key.RightAlt:
                case Avalonia.Input.Key.LeftShift:
                case Avalonia.Input.Key.RightShift:
                case Avalonia.Input.Key.LWin:
                case Avalonia.Input.Key.RWin:
                    return; // Ignore modifier keys by themselves
                default:
                    mainKey = e.Key.ToString();
                    break;
            }

            if (string.IsNullOrEmpty(mainKey)) return;

            // Build hotkey string
            var hotkeyString = modifiers.Count > 0 ? string.Join("+", modifiers) + "+" + mainKey : mainKey;
            
            // Require at least one modifier for global hotkeys
            if (modifiers.Count == 0)
            {
                hotkeyTextBox.Text = "Hotkey must include modifier keys (Ctrl, Alt, Shift, or Win)";
                return;
            }

            hotkeyTextBox.Text = hotkeyString;
            
            // Save the new hotkey
            PersistService.StoreString("hotkey", hotkeyString);
            
            // Try to register the new hotkey
            try
            {
                var avaloniaKey = e.Key;
                var globalModifiers = ConvertToGlobalHotKeyModifiers(e.KeyModifiers);
                var globalKey = ConvertToGlobalHotKeyKey(avaloniaKey);
                
                if (globalKey.HasValue)
                {
                    // Unregister old hotkey and register new one
                    HotKeyService.UnregisterKey();
                    var hotKey = new GlobalHotKey.HotKey(globalKey.Value, globalModifiers);
                    HotKeyService.RegisterHotKey(hotKey);
                    
                    // Ensure the callback is registered
                    App.EnsureHotkeyCallbackRegistered();
                    
                    hotkeyTextBox.Text = hotkeyString + " ✓";
                    System.Diagnostics.Debug.WriteLine($"Registered new hotkey: {hotkeyString}");
                }
                else
                {
                    hotkeyTextBox.Text = hotkeyString + " (Not supported)";
                }
            }
            catch (Exception ex)
            {
                hotkeyTextBox.Text = hotkeyString + " (Failed to register)";
                System.Diagnostics.Debug.WriteLine($"Failed to register hotkey: {ex.Message}");
            }
        }

        private ModifierKeys ConvertToGlobalHotKeyModifiers(KeyModifiers avaloniaModifiers)
        {
            ModifierKeys result = ModifierKeys.None;
            
            if (avaloniaModifiers.HasFlag(KeyModifiers.Control))
                result |= ModifierKeys.Control;
            if (avaloniaModifiers.HasFlag(KeyModifiers.Alt))
                result |= ModifierKeys.Alt;
            if (avaloniaModifiers.HasFlag(KeyModifiers.Shift))
                result |= ModifierKeys.Shift;
            if (avaloniaModifiers.HasFlag(KeyModifiers.Meta))
                result |= ModifierKeys.Windows;
                
            return result;
        }

        private System.Windows.Input.Key? ConvertToGlobalHotKeyKey(Avalonia.Input.Key avaloniaKey)
        {
            // Map common Avalonia keys to System.Windows.Input.Key
            switch (avaloniaKey)
            {
                case Avalonia.Input.Key.F1: return System.Windows.Input.Key.F1;
                case Avalonia.Input.Key.F2: return System.Windows.Input.Key.F2;
                case Avalonia.Input.Key.F3: return System.Windows.Input.Key.F3;
                case Avalonia.Input.Key.F4: return System.Windows.Input.Key.F4;
                case Avalonia.Input.Key.F5: return System.Windows.Input.Key.F5;
                case Avalonia.Input.Key.F6: return System.Windows.Input.Key.F6;
                case Avalonia.Input.Key.F7: return System.Windows.Input.Key.F7;
                case Avalonia.Input.Key.F8: return System.Windows.Input.Key.F8;
                case Avalonia.Input.Key.F9: return System.Windows.Input.Key.F9;
                case Avalonia.Input.Key.F10: return System.Windows.Input.Key.F10;
                case Avalonia.Input.Key.F11: return System.Windows.Input.Key.F11;
                case Avalonia.Input.Key.F12: return System.Windows.Input.Key.F12;
                case Avalonia.Input.Key.A: return System.Windows.Input.Key.A;
                case Avalonia.Input.Key.B: return System.Windows.Input.Key.B;
                case Avalonia.Input.Key.C: return System.Windows.Input.Key.C;
                case Avalonia.Input.Key.D: return System.Windows.Input.Key.D;
                case Avalonia.Input.Key.E: return System.Windows.Input.Key.E;
                case Avalonia.Input.Key.F: return System.Windows.Input.Key.F;
                case Avalonia.Input.Key.G: return System.Windows.Input.Key.G;
                case Avalonia.Input.Key.H: return System.Windows.Input.Key.H;
                case Avalonia.Input.Key.I: return System.Windows.Input.Key.I;
                case Avalonia.Input.Key.J: return System.Windows.Input.Key.J;
                case Avalonia.Input.Key.K: return System.Windows.Input.Key.K;
                case Avalonia.Input.Key.L: return System.Windows.Input.Key.L;
                case Avalonia.Input.Key.M: return System.Windows.Input.Key.M;
                case Avalonia.Input.Key.N: return System.Windows.Input.Key.N;
                case Avalonia.Input.Key.O: return System.Windows.Input.Key.O;
                case Avalonia.Input.Key.P: return System.Windows.Input.Key.P;
                case Avalonia.Input.Key.Q: return System.Windows.Input.Key.Q;
                case Avalonia.Input.Key.R: return System.Windows.Input.Key.R;
                case Avalonia.Input.Key.S: return System.Windows.Input.Key.S;
                case Avalonia.Input.Key.T: return System.Windows.Input.Key.T;
                case Avalonia.Input.Key.U: return System.Windows.Input.Key.U;
                case Avalonia.Input.Key.V: return System.Windows.Input.Key.V;
                case Avalonia.Input.Key.W: return System.Windows.Input.Key.W;
                case Avalonia.Input.Key.X: return System.Windows.Input.Key.X;
                case Avalonia.Input.Key.Y: return System.Windows.Input.Key.Y;
                case Avalonia.Input.Key.Z: return System.Windows.Input.Key.Z;
                case Avalonia.Input.Key.D0: return System.Windows.Input.Key.D0;
                case Avalonia.Input.Key.D1: return System.Windows.Input.Key.D1;
                case Avalonia.Input.Key.D2: return System.Windows.Input.Key.D2;
                case Avalonia.Input.Key.D3: return System.Windows.Input.Key.D3;
                case Avalonia.Input.Key.D4: return System.Windows.Input.Key.D4;
                case Avalonia.Input.Key.D5: return System.Windows.Input.Key.D5;
                case Avalonia.Input.Key.D6: return System.Windows.Input.Key.D6;
                case Avalonia.Input.Key.D7: return System.Windows.Input.Key.D7;
                case Avalonia.Input.Key.D8: return System.Windows.Input.Key.D8;
                case Avalonia.Input.Key.D9: return System.Windows.Input.Key.D9;
                case Avalonia.Input.Key.Space: return System.Windows.Input.Key.Space;
                case Avalonia.Input.Key.Enter: return System.Windows.Input.Key.Enter;
                case Avalonia.Input.Key.Escape: return System.Windows.Input.Key.Escape;
                case Avalonia.Input.Key.Tab: return System.Windows.Input.Key.Tab;
                case Avalonia.Input.Key.Back: return System.Windows.Input.Key.Back;
                case Avalonia.Input.Key.Delete: return System.Windows.Input.Key.Delete;
                case Avalonia.Input.Key.Insert: return System.Windows.Input.Key.Insert;
                case Avalonia.Input.Key.Home: return System.Windows.Input.Key.Home;
                case Avalonia.Input.Key.End: return System.Windows.Input.Key.End;
                case Avalonia.Input.Key.PageUp: return System.Windows.Input.Key.PageUp;
                case Avalonia.Input.Key.PageDown: return System.Windows.Input.Key.PageDown;
                case Avalonia.Input.Key.Up: return System.Windows.Input.Key.Up;
                case Avalonia.Input.Key.Down: return System.Windows.Input.Key.Down;
                case Avalonia.Input.Key.Left: return System.Windows.Input.Key.Left;
                case Avalonia.Input.Key.Right: return System.Windows.Input.Key.Right;
                default: return null;
            }
        }

        private void OnResetHotkeyClicked(object sender, RoutedEventArgs e)
        {
            var hotkeyTextBox = this.FindControl<TextBox>("HotkeyTextBox");
            var defaultHotkey = "Ctrl+Shift+F1";
            
            hotkeyTextBox.Text = defaultHotkey;
            PersistService.StoreString("hotkey", defaultHotkey);
            
            // Register the default hotkey
            try
            {
                HotKeyService.UnregisterKey();
                var hotKey = new GlobalHotKey.HotKey(System.Windows.Input.Key.F5, ModifierKeys.Control | ModifierKeys.Alt);
                HotKeyService.RegisterHotKey(hotKey);
                
                // Ensure the callback is registered
                App.EnsureHotkeyCallbackRegistered();
                
                hotkeyTextBox.Text = defaultHotkey + " ✓";
            }
            catch (Exception ex)
            {
                hotkeyTextBox.Text = defaultHotkey + " (Failed to register)";
                System.Diagnostics.Debug.WriteLine($"Failed to register default hotkey: {ex.Message}");
            }
        }

        private string GetDefaultDeviceName()
        {
            var defaultDevice = this.audioServiceAdapter.GetDefaultPlaybackDevice();
            return defaultDevice?.Name;
        }

        public void OnCloseClicked(object sender, EventArgs args)
        {
            this.Hide();
        }
    }
}
