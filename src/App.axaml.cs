using System;
using System.IO;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using GlobalHotKey;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AudioToggle
{
    public partial class App : Application
    {
        private AudioServiceAdapter audioService;
        private IUpdateService updateService;
        
        public override void Initialize()
        {
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, "Avalonia Infrastructure");
            Debug.WriteLine("System Diagnostics Debug");

            audioService = new AudioServiceAdapter();
            updateService = new UpdateService();
            
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
                    Debug.WriteLine($"Registered output hotkey: {savedHotkey}");
                }
                else
                {
                    Debug.WriteLine($"Failed to parse output hotkey: {savedHotkey}");
                    // Fallback to default
                    var hotKey = new HotKey(Key.F1, ModifierKeys.Control | ModifierKeys.Shift);
                    hotKeyService.RegisterOutputHotKey(hotKey);
                    hotKeyService.RegisterOutputCallback(OnOutputHotKeyPressed);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering output hotkey: {ex.Message}");
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
                    Debug.WriteLine($"Registered input hotkey: {savedHotkey}");
                }
                else
                {
                    Debug.WriteLine($"Failed to parse input hotkey: {savedHotkey}");
                    // Fallback to default
                    var hotKey = new HotKey(Key.F2, ModifierKeys.Control | ModifierKeys.Shift);
                    hotKeyService.RegisterInputHotKey(hotKey);
                    hotKeyService.RegisterInputCallback(OnInputHotKeyPressed);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering input hotkey: {ex.Message}");
            }
        }

        private void OnHotKeyPressed()
        {
            try
            {
                var enabledDevices = audioService.GetEnabledDevicesForCycling();
                if (enabledDevices.Count == 0)
                {
                    Debug.WriteLine("No enabled devices for cycling");
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
                    Debug.WriteLine($"Switched to audio device: {nextDevice}");
                    
                    // Show notification
                    NotificationService.ShowDeviceNotification(nextDevice);
                    
                    // Update the settings window UI if it exists
                    SettingsWindow.UpdateDefaultDevice(nextDevice);
                }
                else
                {
                    Debug.WriteLine($"Failed to switch to audio device: {nextDevice}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in hotkey callback: {ex.Message}");
            }
        }

        private void OnOutputHotKeyPressed()
        {
            try
            {
                var enabledDevices = audioService.GetEnabledDevicesForCycling();
                if (enabledDevices.Count == 0)
                {
                    Debug.WriteLine("No enabled output devices for cycling");
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
                    Debug.WriteLine($"Switched to output device: {nextDevice}");
                    
                    // Show notification
                    NotificationService.ShowDeviceNotification(nextDevice);
                    
                    // Update the settings window UI if it exists
                    SettingsWindow.UpdateDefaultDevice(nextDevice);
                }
                else
                {
                    Debug.WriteLine($"Failed to switch to output device: {nextDevice}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in output hotkey callback: {ex.Message}");
            }
        }

        private void OnInputHotKeyPressed()
        {
            try
            {
                var enabledDevices = audioService.GetEnabledInputDevicesForCycling();
                if (enabledDevices.Count == 0)
                {
                    Debug.WriteLine("No enabled input devices for cycling");
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
                    Debug.WriteLine($"Switched to input device: {nextDevice}");
                    
                    // Show notification
                    NotificationService.ShowDeviceNotification($"Input: {nextDevice}");
                    
                    // Update the settings window UI if it exists
                    SettingsWindow.UpdateDefaultInputDevice(nextDevice);
                }
                else
                {
                    Debug.WriteLine($"Failed to switch to input device: {nextDevice}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in input hotkey callback: {ex.Message}");
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

            // Check for updates if enabled
            _ = CheckForUpdatesAsync();

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

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                // Check if auto-update is enabled
                var autoUpdateEnabled = PersistService.GetBool("autoUpdateEnabled", true);
                if (!autoUpdateEnabled) return;

                // Check if we already checked today
                var lastCheckDate = PersistService.GetString("lastUpdateCheck", "");
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                if (lastCheckDate == today) return;

                var updateInfo = await updateService.CheckForUpdatesAsync();
                if (updateInfo != null && updateService.IsUpdateAvailable(updateInfo))
                {
                    // Show update notification
                    ShowUpdateNotification(updateInfo);
                }

                // Update last check date
                PersistService.StoreString("lastUpdateCheck", today);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for updates: {ex.Message}");
            }
        }

        private void ShowUpdateNotification(UpdateInfo updateInfo)
        {
            try
            {
                var notificationWindow = new Window
                {
                    Title = "Update Available",
                    Width = 400,
                    Height = 250,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    CanResize = false,
                    Icon = new WindowIcon("avares://AudioToggle/assets/tray_icon.ico")
                };

                var stackPanel = new StackPanel { Margin = new Avalonia.Thickness(20) };

                stackPanel.Children.Add(new TextBlock
                {
                    Text = "AudioToggle Update Available",
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    FontSize = 16,
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                });

                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Version {updateInfo.Version} is available",
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                });

                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Published: {updateInfo.PublishedAt.ToShortDateString()}",
                    FontSize = 12,
                    Foreground = Avalonia.Media.Brushes.Gray,
                    Margin = new Avalonia.Thickness(0, 0, 0, 15)
                });

                var buttonPanel = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Margin = new Avalonia.Thickness(0, 10, 0, 0)
                };

                var downloadButton = new Button
                {
                    Content = "Download",
                    Margin = new Avalonia.Thickness(0, 0, 10, 0),
                    Padding = new Avalonia.Thickness(20, 5, 20, 5)
                };

                var laterButton = new Button
                {
                    Content = "Later",
                    Padding = new Avalonia.Thickness(20, 5, 20, 5)
                };

                downloadButton.Click += async (s, e) =>
                {
                    notificationWindow.Close();
                    await DownloadAndInstallUpdateAsync(updateInfo);
                };

                laterButton.Click += (s, e) => notificationWindow.Close();

                buttonPanel.Children.Add(downloadButton);
                buttonPanel.Children.Add(laterButton);
                stackPanel.Children.Add(buttonPanel);

                notificationWindow.Content = stackPanel;
                notificationWindow.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing update notification: {ex.Message}");
            }
        }

        private async Task DownloadAndInstallUpdateAsync(UpdateInfo updateInfo)
        {
            try
            {
                var updateService = new UpdateService();
                var downloadPath = Path.Combine(Path.GetTempPath(), $"AudioToggle-{updateInfo.Version}.zip");

                // Show download progress
                var progressWindow = new Window
                {
                    Title = "Downloading Update",
                    Width = 300,
                    Height = 100,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    CanResize = false
                };

                var progressText = new TextBlock
                {
                    Text = "Downloading update...",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                progressWindow.Content = progressText;
                progressWindow.Show();

                await updateService.DownloadUpdateAsync(updateInfo, downloadPath);

                progressWindow.Close();

                // Get the current application directory
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var appExecutable = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(appExecutable))
                {
                    appExecutable = Path.Combine(appDirectory, "AudioToggle.exe");
                }

                // Extract ZIP file to temp location first
                var extractPath = Path.Combine(Path.GetTempPath(), $"AudioToggle-{updateInfo.Version}");
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Directory.CreateDirectory(extractPath);

                System.IO.Compression.ZipFile.ExtractToDirectory(downloadPath, extractPath);

                // Copy all files from extracted ZIP to application directory
                foreach (var file in Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(extractPath, file);
                    var targetPath = Path.Combine(appDirectory, relativePath);

                    // Ensure target directory exists
                    var targetDir = Path.GetDirectoryName(targetPath);
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    // Copy file (this will overwrite existing files)
                    File.Copy(file, targetPath, true);
                    System.Diagnostics.Debug.WriteLine($"Updated file: {relativePath}");
                }

                // Create a restart script
                var restartScript = Path.Combine(appDirectory, "restart.bat");
                var scriptContent = $@"
@echo off
timeout /t 2 /nobreak > nul
start "" ""{appExecutable}""
del ""%~f0""
";
                File.WriteAllText(restartScript, scriptContent);

                // Launch the restart script and exit
                Process.Start(new ProcessStartInfo
                {
                    FileName = restartScript,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                // Exit current application
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading update: {ex.Message}");
                // Show error notification
                var errorWindow = new Window
                {
                    Title = "Download Failed",
                    Width = 300,
                    Height = 120,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    CanResize = false
                };

                var errorPanel = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                errorPanel.Children.Add(new TextBlock
                {
                    Text = "Failed to download update.",
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                });

                var okButton = new Button
                {
                    Content = "OK",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                okButton.Click += (s, e) => errorWindow.Close();
                errorPanel.Children.Add(okButton);

                errorWindow.Content = errorPanel;
                errorWindow.Show();
            }
        }
    }
}
