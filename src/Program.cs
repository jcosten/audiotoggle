using System;
using System.Threading;
using System.Runtime;
using Avalonia;
using Avalonia.Controls;

namespace AudioToggle
{
    class Program
    {
        private static Mutex mutex = null;
        private const string MutexName = "AudioToggle_SingleInstance_Mutex";

        [STAThread]
        public static void Main(string[] args)
        {
            // Optimize GC for low memory footprint
            GCSettings.LatencyMode = GCLatencyMode.Interactive;
            
            // Try to create a named mutex to ensure single instance
            bool createdNew;
            mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                // Another instance is already running
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debug.WriteLine("Another instance of AudioToggle is already running.");
                return;
            }

            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
            }
            finally
            {
                // Release the mutex when the application exits
                mutex?.ReleaseMutex();
                mutex?.Dispose();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(Avalonia.Logging.LogEventLevel.Error);
    }
}