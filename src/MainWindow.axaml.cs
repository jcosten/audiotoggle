using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioToggle
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            System.Diagnostics.Debug.WriteLine("MainWindow constructor called");
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }
    }
}
