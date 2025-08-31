using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;

namespace AudioToggle
{
    class Program
    {
        // Mutex to prevent multiple instances
        private static Mutex _singleInstanceMutex;

        [STAThread]
        public static void Main(string[] args)
        {
            // Create a named mutex to ensure only one instance runs
            const string mutexName = "AudioToggle_SingleInstanceMutex";
            
            try
            {
                // Try to create the mutex
                _singleInstanceMutex = new Mutex(true, mutexName, out bool createdNew);
                
                if (!createdNew)
                {
                    // Another instance is already running
                    return;
                }
                
                // No other instance running, proceed with application
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
            }
            catch (Exception)
            {
                // If mutex creation fails, allow the application to run anyway
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(Avalonia.Logging.LogEventLevel.Error);
    }
}