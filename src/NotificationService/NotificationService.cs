using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Input;

namespace AudioToggle
{
    public static class NotificationService
    {
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        // Auto-hide delay in milliseconds (5 seconds)
        private const int AutoHideDelayMs = 5000;

        private static Window _window;
        private static Task _autoHideTask;

        public static void ShowDeviceNotification(string deviceName)
        {
            // Check if notifications are enabled
            if (!PersistService.GetBool("showNotifications", true))
            {
                return; // Notifications are disabled
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Cancel any existing auto-hide
                if (_autoHideTask != null && !_autoHideTask.IsCompleted)
                {
                    // Task will be cancelled naturally
                }

                // Create window if needed
                if (_window == null)
                {
                    CreateWindow();
                }

                // Update content
                UpdateContent(deviceName);

                // Show window
                if (!_window.IsVisible)
                {
                    _window.Show();
                }

                // Start auto-hide timer
                StartAutoHideTimer();
            });
        }

        private static void CreateWindow()
        {
            _window = new Window
            {
                Title = "",
                Width = 300,
                Height = 80,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Topmost = true,
                ShowInTaskbar = false,
                SystemDecorations = SystemDecorations.None,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur },
                CornerRadius = new CornerRadius(8)
            };

            // Position in bottom right
            int screenWidth = GetSystemMetrics(0);
            int screenHeight = GetSystemMetrics(1);
            int x = screenWidth - (300 + 120); // 300 + 120 margin
            int y = screenHeight - (80 + 90); // 80 + 90 margin
            _window.Position = new PixelPoint(Math.Max(0, x), Math.Max(0, y));

            // Create content
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            // Add hover effect
            border.PointerEntered += (s, e) =>
            {
                if (s is Border b)
                {
                    b.Background = new SolidColorBrush(Color.FromRgb(55, 55, 58));
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 122, 204));
                }
            };

            border.PointerExited += (s, e) =>
            {
                if (s is Border b)
                {
                    b.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70));
                }
            };

            // Click to hide
            border.PointerPressed += (s, e) => HideNotification();

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock
            {
                Text = "🔊 Audio Toggle",
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Margin = new Thickness(0, 0, 0, 8)
            });
            stackPanel.Children.Add(new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 12
            });

            border.Child = stackPanel;
            _window.Content = border;
        }

        private static void UpdateContent(string deviceName)
        {
            if (_window?.Content is Border border && border.Child is StackPanel stackPanel)
            {
                if (stackPanel.Children.Count >= 2 && stackPanel.Children[1] is TextBlock deviceTextBlock)
                {
                    deviceTextBlock.Text = deviceName;
                }
            }
        }

        private static void StartAutoHideTimer()
        {
            _autoHideTask = Task.Delay(AutoHideDelayMs).ContinueWith(_ =>
            {
                Dispatcher.UIThread.InvokeAsync(() => HideNotification());
            });
        }

        private static void HideNotification()
        {
            if (_window?.IsVisible == true)
            {
                _window.Hide();
            }
        }
    }
}