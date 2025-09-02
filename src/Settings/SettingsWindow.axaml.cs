using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace AudioToggle
{

    public partial class SettingsWindow : Window
    {
        private readonly AudioServiceAdapter audioServiceAdapter;
        private static IHotKeyService hotKeyService = new HotKeyServiceAdapter();
        private List<DeviceViewModel> deviceViewModels;
        private List<DeviceViewModel> inputDeviceViewModels;
        private static SettingsWindow _instance;

        public SettingsWindow()
        {
            this.audioServiceAdapter = new AudioServiceAdapter();
            AvaloniaXamlLoader.Load(this);

            InitializeOutputDevicesTab();
            InitializeInputDevicesTab();
            InitializeGeneralTab();

            this.Closing += (sender, e) => {
                e.Cancel = true;
                this.Hide();
            };

            Debug.WriteLine("SettingsWindow instance created");
        }

        // Static method to get or create the settings window instance
        public static SettingsWindow GetInstance()
        {
            if (_instance == null)
            {
                Debug.WriteLine("Creating new SettingsWindow instance");
                _instance = new SettingsWindow();
            }
            else
            {
                Debug.WriteLine("Reusing existing SettingsWindow instance");
            }
            return _instance;
        }

        // Static method to show the settings window (ensures only one instance)
        public static void ShowInstance()
        {
            Debug.WriteLine("ShowInstance called");
            var instance = GetInstance();

            // Ensure the window is visible and activated
            if (!instance.IsVisible)
            {
                instance.Show();
            }

            instance.Activate();
            instance.WindowState = WindowState.Normal;
            instance.Topmost = true;
            instance.Topmost = false; // Reset topmost to bring to front
            instance.Focus();

            Debug.WriteLine("SettingsWindow shown and activated");
        }

        // Static method to hide the settings window
        public static void HideInstance()
        {
            if (_instance != null)
            {
                _instance.Hide();
            }
        }

        // Static method to check if instance exists and is visible
        public static bool IsInstanceVisible()
        {
            return _instance != null && _instance.IsVisible;
        }

        // Static method to update the default device from external classes
        public static void UpdateDefaultDevice(string newDefaultDeviceName)
        {
            if (_instance != null)
            {
                _instance.RefreshDefaultDevice(newDefaultDeviceName);
            }
        }

        // Static method to update the default input device from external classes
        public static void UpdateDefaultInputDevice(string newDefaultDeviceName)
        {
            if (_instance != null)
            {
                _instance.RefreshInputDefaultDevice(newDefaultDeviceName);
            }
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

        private void InitializeOutputDevicesTab()
        {
            var listBox = this.FindControl<ItemsControl>("AudioDevicesListBox");

            // Invalidate cache to ensure we get fresh device information
            this.audioServiceAdapter.InvalidateCache();

            var deviceNames = this.audioServiceAdapter.GetOutputDeviceNames();
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

            // Initialize hotkey settings
            var resetOutputHotkeyButton = this.FindControl<Button>("ResetOutputHotkeyButton");
            var outputHotkeyTextBox = this.FindControl<TextBox>("OutputHotkeyTextBox");

            // Load saved hotkey or use default
            var savedOutputHotkey = PersistService.GetString("outputHotkey", "Ctrl+F1");
            outputHotkeyTextBox.Text = savedOutputHotkey;

            // Handle key input for hotkey capture
            outputHotkeyTextBox.KeyDown += OnOutputHotkeyTextBoxKeyDown;
            outputHotkeyTextBox.GotFocus += (s, e) => 
            {
                outputHotkeyTextBox.Text = "Press key combination...";
                outputHotkeyTextBox.SelectAll();
            };
            outputHotkeyTextBox.LostFocus += (s, e) =>
            {
                if (outputHotkeyTextBox.Text == "Press key combination...")
                {
                    outputHotkeyTextBox.Text = savedOutputHotkey;
                }
            };

            resetOutputHotkeyButton.Click += OnResetOutputHotkeyClicked;
        }

        private void InitializeInputDevicesTab()
        {
            var inputListBox = this.FindControl<ItemsControl>("InputDevicesListBox");

            // Invalidate cache to ensure we get fresh device information
            this.audioServiceAdapter.InvalidateCache();
            
            var inputDeviceNames = this.audioServiceAdapter.GetInputDeviceNames();
            var defaultInputDevice = GetDefaultInputDeviceName();
            var enabledInputDevices = GetEnabledInputDevices();
            inputDeviceViewModels = new List<DeviceViewModel>();
            
            foreach (var name in inputDeviceNames)
            {
                var deviceViewModel = new DeviceViewModel
                {
                    Name = name,
                    IsDefault = name == defaultInputDevice,
                    IsEnabled = enabledInputDevices.Contains(name)
                };
                
                // Subscribe to changes in the IsEnabled property
                deviceViewModel.EnabledChanged += (s, e) => SaveEnabledInputDevices(inputDeviceViewModels);
                inputDeviceViewModels.Add(deviceViewModel);
            }
            
            inputListBox.ItemsSource = inputDeviceViewModels;

            // Initialize input hotkey settings
            var resetInputHotkeyButton = this.FindControl<Button>("ResetInputHotkeyButton");
            var inputHotkeyTextBox = this.FindControl<TextBox>("InputHotkeyTextBox");

            // Load saved hotkey or use default
            var savedInputHotkey = PersistService.GetString("inputHotkey", "Ctrl+F2");
            inputHotkeyTextBox.Text = savedInputHotkey;

            // Handle key input for hotkey capture
            inputHotkeyTextBox.KeyDown += OnInputHotkeyTextBoxKeyDown;
            inputHotkeyTextBox.GotFocus += (s, e) => 
            {
                inputHotkeyTextBox.Text = "Press key combination...";
                inputHotkeyTextBox.SelectAll();
            };
            inputHotkeyTextBox.LostFocus += (s, e) =>
            {
                if (inputHotkeyTextBox.Text == "Press key combination...")
                {
                    inputHotkeyTextBox.Text = savedInputHotkey;
                }
            };

            resetInputHotkeyButton.Click += OnResetInputHotkeyClicked;
        }

        private List<string> GetEnabledInputDevices()
        {
            var enabledDevicesJson = PersistService.GetString("enabledInputDevices", "[]");
            try
            {
                return JsonSerializer.Deserialize(enabledDevicesJson, typeof(List<string>)) as List<string> ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void SaveEnabledInputDevices(List<DeviceViewModel> devices)
        {
            var enabledDevices = devices.Where(d => d.IsEnabled).Select(d => d.Name).ToList();

            // Use human-readable JSON formatting
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = System.Text.Json.JsonSerializer.Serialize(enabledDevices, options);
            PersistService.StoreString("enabledInputDevices", json);
        }

        private void InputDeviceText_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is TextBlock textBlock && inputDeviceViewModels != null)
            {
                // Find the device view model that corresponds to this text block
                var deviceName = textBlock.Text;
                var deviceViewModel = inputDeviceViewModels.FirstOrDefault(d => d.Name == deviceName);
                
                if (deviceViewModel != null)
                {
                    // Toggle the IsEnabled property
                    deviceViewModel.IsEnabled = !deviceViewModel.IsEnabled;
                }
            }
        }

        private string GetDefaultInputDeviceName()
        {
            var defaultDevice = this.audioServiceAdapter.GetDefaultInputDevice();
            return defaultDevice?.Name;
        }

        private void RefreshInputDefaultDevice(string newDefaultDeviceName)
        {
            if (inputDeviceViewModels != null)
            {
                // Update the IsDefault property for all devices
                foreach (var device in inputDeviceViewModels)
                {
                    device.IsDefault = device.Name == newDefaultDeviceName;
                }
            }
        }

        private List<string> GetEnabledDevices()
        {
            var enabledDevicesJson = PersistService.GetString("enabledDevices", "[]");
            try
            {
                return JsonSerializer.Deserialize(enabledDevicesJson, typeof(List<string>)) as List<string> ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void SaveEnabledDevices(List<DeviceViewModel> devices)
        {
            var enabledDevices = devices.Where(d => d.IsEnabled).Select(d => d.Name).ToList();

            // Use human-readable JSON formatting
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = System.Text.Json.JsonSerializer.Serialize(enabledDevices, options);
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
                    Debug.WriteLine($"Startup setting changed to: {isChecked}");
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

        private void OnOutputHotkeyTextBoxKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
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
            PersistService.StoreString("outputHotkey", hotkeyString);
            
            // Try to register the new hotkey
            try
            {
                var avaloniaKey = e.Key;
                var globalModifiers = ConvertToGlobalHotKeyModifiers(e.KeyModifiers);
                var globalKey = hotKeyService.ConvertToGlobalHotKeyKey(avaloniaKey);

                if (globalKey.HasValue)
                {
                    // Unregister old hotkey and register new one
                    hotKeyService.UnregisterOutputHotKey();
                    var hotKey = new GlobalHotKey.HotKey(globalKey.Value, globalModifiers);
                    hotKeyService.RegisterOutputHotKey(hotKey);
                    
                    // Ensure the callback is registered
                    App.EnsureOutputHotkeyCallbackRegistered();
                    
                    hotkeyTextBox.Text = hotkeyString + " ✓";
                    Debug.WriteLine($"Registered new output hotkey: {hotkeyString}");
                }
                else
                {
                    hotkeyTextBox.Text = hotkeyString + " (Not supported)";
                }
            }
            catch (Exception ex)
            {
                hotkeyTextBox.Text = hotkeyString + " (Failed to register)";
                Debug.WriteLine($"Failed to register output hotkey: {ex.Message}");
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


        private void OnResetOutputHotkeyClicked(object sender, RoutedEventArgs e)
        {
            var outputHotkeyTextBox = this.FindControl<TextBox>("OutputHotkeyTextBox");
            var defaultHotKey = new GlobalHotKey.HotKey(System.Windows.Input.Key.F1, ModifierKeys.Control | ModifierKeys.Shift);
            // convert defaultHotkey to string in human readable format
            var defaultHotkeyString = hotKeyService.ConvertToString(defaultHotKey);

            outputHotkeyTextBox.Text = defaultHotkeyString;
            PersistService.StoreString("outputHotkey", defaultHotkeyString);

            // Register the default output hotkey
            try
            {
                hotKeyService.UnregisterOutputHotKey();
                hotKeyService.RegisterOutputHotKey(defaultHotKey);
                
                // Ensure the callback is registered
                App.EnsureOutputHotkeyCallbackRegistered();

                outputHotkeyTextBox.Text = defaultHotkeyString + " ✓";
            }
            catch (Exception ex)
            {
                outputHotkeyTextBox.Text = defaultHotkeyString + " (Failed to register)";
                Debug.WriteLine($"Failed to register default output hotkey: {ex.Message}");
            }
        }

        private void OnInputHotkeyTextBoxKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            e.Handled = true;
            
            var inputHotkeyTextBox = sender as TextBox;
            if (inputHotkeyTextBox == null) return;

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
                inputHotkeyTextBox.Text = "Hotkey must include modifier keys (Ctrl, Alt, Shift, or Win)";
                return;
            }

            inputHotkeyTextBox.Text = hotkeyString;
            
            // Save the new hotkey
            PersistService.StoreString("inputHotkey", hotkeyString);
            
            // Try to register the new hotkey
            try
            {
                var avaloniaKey = e.Key;
                var globalModifiers = ConvertToGlobalHotKeyModifiers(e.KeyModifiers);
                var globalKey = hotKeyService.ConvertToGlobalHotKeyKey(avaloniaKey);

                if (globalKey.HasValue)
                {
                    // Unregister old hotkey and register new one
                    hotKeyService.UnregisterInputHotKey();
                    var hotKey = new GlobalHotKey.HotKey(globalKey.Value, globalModifiers);
                    hotKeyService.RegisterInputHotKey(hotKey);
                    
                    // Ensure the callback is registered
                    App.EnsureInputHotkeyCallbackRegistered();
                    
                    inputHotkeyTextBox.Text = hotkeyString + " ✓";
                    Debug.WriteLine($"Registered new input hotkey: {hotkeyString}");
                }
                else
                {
                    inputHotkeyTextBox.Text = hotkeyString + " (Not supported)";
                }
            }
            catch (Exception ex)
            {
                inputHotkeyTextBox.Text = hotkeyString + " (Failed to register)";
                Debug.WriteLine($"Failed to register input hotkey: {ex.Message}");
            }
        }

        private void OnResetInputHotkeyClicked(object sender, RoutedEventArgs e)
        {
            var inputHotkeyTextBox = this.FindControl<TextBox>("InputHotkeyTextBox");
            var defaultHotKey = new GlobalHotKey.HotKey(System.Windows.Input.Key.F2, ModifierKeys.Control | ModifierKeys.Shift);
            // convert defaultHotkey to string in human readable format
            var defaultHotkeyString = hotKeyService.ConvertToString(defaultHotKey);

            inputHotkeyTextBox.Text = defaultHotkeyString;
            PersistService.StoreString("inputHotkey", defaultHotkeyString);

            // Register the default input hotkey
            try
            {
                hotKeyService.UnregisterInputHotKey();
                hotKeyService.RegisterInputHotKey(defaultHotKey);
                
                // Ensure the callback is registered
                App.EnsureInputHotkeyCallbackRegistered();

                inputHotkeyTextBox.Text = defaultHotkeyString + " ✓";
            }
            catch (Exception ex)
            {
                inputHotkeyTextBox.Text = defaultHotkeyString + " (Failed to register)";
                Debug.WriteLine($"Failed to register default input hotkey: {ex.Message}");
            }
        }

        private string GetDefaultDeviceName()
        {
            var defaultDevice = this.audioServiceAdapter.GetDefaultOutputDevice();
            return defaultDevice?.Name;
        }

        private void GitHubLinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://github.com/jcosten/audiotoggle/",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open GitHub URL: {ex.Message}");
            }
        }

        public void OnCloseClicked(object sender, EventArgs args)
        {
            SettingsWindow.HideInstance();
        }
    }
}
