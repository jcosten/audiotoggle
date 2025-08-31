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
        private static TextBlock _deviceTextBlock; // Cache reference to avoid searching

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
                _autoHideTask?.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnCanceled);

                // Create window if needed (lazy initialization)
                if (_window == null)
                {
                    CreateWindow();
                }

                // Update content efficiently
                if (_deviceTextBlock != null)
                {
                    _deviceTextBlock.Text = deviceName;
                }

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

            // Create content with minimal allocations
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            // Optimize hover effects with pre-created brushes
            var normalBackground = new SolidColorBrush(Color.FromRgb(45, 45, 48));
            var normalBorder = new SolidColorBrush(Color.FromRgb(63, 63, 70));
            var hoverBackground = new SolidColorBrush(Color.FromRgb(55, 55, 58));
            var hoverBorder = new SolidColorBrush(Color.FromRgb(100, 122, 204));

            border.PointerEntered += (s, e) =>
            {
                if (s is Border b)
                {
                    b.Background = hoverBackground;
                    b.BorderBrush = hoverBorder;
                }
            };

            border.PointerExited += (s, e) =>
            {
                if (s is Border b)
                {
                    b.Background = normalBackground;
                    b.BorderBrush = normalBorder;
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
            
            _deviceTextBlock = new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 12
            };
            stackPanel.Children.Add(_deviceTextBlock);

            border.Child = stackPanel;
            _window.Content = border;
        }

        private static void UpdateContent(string deviceName)
        {
            // This method is no longer needed as we update directly
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