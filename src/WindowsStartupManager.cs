using System;
using Microsoft.Win32;

namespace AudioToggle
{
    public static class WindowsStartupManager
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string ApplicationName = "AudioToggle";

        /// <summary>
        /// Checks if the application is set to start with Windows
        /// </summary>
        /// <returns>True if startup is enabled, false otherwise</returns>
        public static bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                var value = key?.GetValue(ApplicationName);
                return value != null && !string.IsNullOrEmpty(value.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking startup status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Enables or disables startup with Windows
        /// </summary>
        /// <param name="enable">True to enable startup, false to disable</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SetStartupEnabled(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to open registry key for writing");
                    return false;
                }

                if (enable)
                {
                    var exePath = GetApplicationPath();
                    if (string.IsNullOrEmpty(exePath))
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to determine application path");
                        return false;
                    }

                    key.SetValue(ApplicationName, $"\"{exePath}\"");
                    System.Diagnostics.Debug.WriteLine($"Added to startup: {exePath}");
                }
                else
                {
                    key.DeleteValue(ApplicationName, false);
                    System.Diagnostics.Debug.WriteLine("Removed from startup");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting startup status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the current application's executable path
        /// </summary>
        /// <returns>Full path to the executable, or null if not found</returns>
        private static string GetApplicationPath()
        {
            try
            {
                // Try to get the path from the current process
                var process = System.Diagnostics.Process.GetCurrentProcess();
                return process.MainModule?.FileName;
            }
            catch
            {
                try
                {
                    // Fallback to assembly location
                    return System.Reflection.Assembly.GetExecutingAssembly().Location;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets detailed startup information for diagnostics
        /// </summary>
        /// <returns>Startup information object</returns>
        public static StartupInfo GetStartupInfo()
        {
            var info = new StartupInfo
            {
                IsEnabled = IsStartupEnabled(),
                ApplicationPath = GetApplicationPath()
            };

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                var value = key?.GetValue(ApplicationName);
                info.RegistryValue = value?.ToString();
            }
            catch (Exception ex)
            {
                info.Error = ex.Message;
            }

            return info;
        }
    }

    public class StartupInfo
    {
        public bool IsEnabled { get; set; }
        public string ApplicationPath { get; set; }
        public string RegistryValue { get; set; }
        public string Error { get; set; }

        public override string ToString()
        {
            return $"Enabled: {IsEnabled}, Path: {ApplicationPath}, Registry: {RegistryValue}, Error: {Error}";
        }
    }
}
