using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace AudioToggle
{

    public partial class SettingsWindow : Window
    {
        private readonly AudioServiceAdapter audioServiceAdapter;
        private static IHotKeyService hotKeyService = new HotKeyServiceAdapter();
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
            var listBox = this.FindControl<ItemsControl>("AudioDevicesListBox");

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

        private void DeviceText_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is TextBlock textBlock && deviceViewModels != null)
            {
                // Find the device view model that corresponds to this text block
                var deviceName = textBlock.Text;
                var deviceViewModel = deviceViewModels.FirstOrDefault(d => d.Name == deviceName);
                
                if (deviceViewModel != null)
                {
                    // Toggle the IsEnabled property
                    deviceViewModel.IsEnabled = !deviceViewModel.IsEnabled;
                }
            }
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
            string mainKey;
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
                var globalKey = hotKeyService.ConvertToGlobalHotKeyKey(avaloniaKey);

                if (globalKey.HasValue)
                {
                    // Unregister old hotkey and register new one
                    hotKeyService.UnregisterKey();
                    var hotKey = new GlobalHotKey.HotKey(globalKey.Value, globalModifiers);
                    hotKeyService.RegisterHotKey(hotKey);
                    
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


        private void OnResetHotkeyClicked(object sender, RoutedEventArgs e)
        {
            var hotkeyTextBox = this.FindControl<TextBox>("HotkeyTextBox");
            var defaultHotKey = new GlobalHotKey.HotKey(System.Windows.Input.Key.F1, ModifierKeys.Control | ModifierKeys.Shift);
            // convert defaultHotkey to string in human readable format
            var defaultHotkeyString = hotKeyService.ConvertToString(defaultHotKey);

            hotkeyTextBox.Text = defaultHotkeyString;
            PersistService.StoreString("hotkey", defaultHotkeyString);

            // Register the default hotkey
            try
            {
                hotKeyService.UnregisterKey();
                hotKeyService.RegisterHotKey(defaultHotKey);
                
                // Ensure the callback is registered
                App.EnsureHotkeyCallbackRegistered();

                hotkeyTextBox.Text = defaultHotkeyString + " ✓";
            }
            catch (Exception ex)
            {
                hotkeyTextBox.Text = defaultHotkeyString + " (Failed to register)";
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
