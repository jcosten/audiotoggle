using System;
using Avalonia;
using Avalonia.Controls;

namespace AudioToggle
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
          .StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(Avalonia.Logging.LogEventLevel.Error);
    }
}