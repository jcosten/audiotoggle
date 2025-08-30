using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;

namespace AudioToggle
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            // Configure for tray-only operation
            this.ShowInTaskbar = false;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            // Hide the window immediately after it opens
            Dispatcher.UIThread.Post(() => this.Hide());
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }
    }
}
