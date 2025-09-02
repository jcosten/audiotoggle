using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AudioToggle
{
    public static class PersistService
    {
        // Stores a string value under a key in a JSON file next to the binaries
        public static void StoreString(string key, string value)
        {
            string filePath = GetJsonFilePath();
            var dict = File.Exists(filePath)
                ? JsonSerializer.Deserialize(File.ReadAllText(filePath), typeof(Dictionary<string, string>)) as Dictionary<string, string>
                : new Dictionary<string, string>();
            dict[key] = value;

            // Use human-readable JSON formatting
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            File.WriteAllText(filePath, JsonSerializer.Serialize(dict, options));
        }

        // Retrieves a string value by key from the JSON file
        public static string GetString(string key)
        {
            string filePath = GetJsonFilePath();
            if (!File.Exists(filePath)) return null;
            var dict = JsonSerializer.Deserialize(File.ReadAllText(filePath), typeof(Dictionary<string, string>)) as Dictionary<string, string>;
            return dict != null && dict.ContainsKey(key) ? dict[key] : null;
        }

        // Retrieves a string value by key from the JSON file with default value
        public static string GetString(string key, string defaultValue)
        {
            var value = GetString(key);
            return value ?? defaultValue;
        }

        // Stores a boolean value under a key in the JSON file
        public static void StoreBool(string key, bool value)
        {
            StoreString(key, value.ToString());
        }

        // Retrieves a boolean value by key from the JSON file
        public static bool GetBool(string key, bool defaultValue = false)
        {
            var value = GetString(key);
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        /// <summary>
        /// Loads settings from the JSON file into a Settings object
        /// </summary>
        public static Settings LoadSettings()
        {
            string filePath = GetJsonFilePath();
            if (!File.Exists(filePath))
            {
                return new Settings();
            }

            try
            {
                // Try to load as structured Settings object first
                var settingsJson = File.ReadAllText(filePath);
                var settings = JsonSerializer.Deserialize<Settings>(settingsJson);

                // If deserialization succeeds and we have structured data, return it
                if (settings != null)
                {
                    return settings;
                }
            }
            catch (JsonException)
            {
                // If structured loading fails, fall back to key-value loading
            }

            // Fallback: Load from key-value pairs (backward compatibility)
            return LoadSettingsFromKeyValue();
        }

        /// <summary>
        /// Saves a Settings object to the JSON file
        /// </summary>
        public static void SaveSettings(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            string filePath = GetJsonFilePath();

            // Use human-readable JSON formatting
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            File.WriteAllText(filePath, JsonSerializer.Serialize(settings, options));
        }

        /// <summary>
        /// Loads settings from key-value pairs (for backward compatibility)
        /// </summary>
        private static Settings LoadSettingsFromKeyValue()
        {
            var settings = new Settings();

            // Load individual settings from key-value store
            settings.OutputHotkey = GetString("outputHotkey", settings.OutputHotkey);
            settings.InputHotkey = GetString("inputHotkey", settings.InputHotkey);
            settings.StartWithWindows = GetBool("startWithWindows", settings.StartWithWindows);
            settings.ShowNotifications = GetBool("showNotifications", settings.ShowNotifications);
            settings.DefaultPlayback = GetString("defaultPlayback", settings.DefaultPlayback);
            settings.DefaultInput = GetString("defaultInput", settings.DefaultInput);

            // Load enabled devices lists
            var enabledDevicesJson = GetString("enabledDevices", "[]");
            try
            {
                settings.EnabledDevices = JsonSerializer.Deserialize<List<string>>(enabledDevicesJson) ?? new List<string>();
            }
            catch
            {
                settings.EnabledDevices = new List<string>();
            }

            var enabledInputDevicesJson = GetString("enabledInputDevices", "[]");
            try
            {
                settings.EnabledInputDevices = JsonSerializer.Deserialize<List<string>>(enabledInputDevicesJson) ?? new List<string>();
            }
            catch
            {
                settings.EnabledInputDevices = new List<string>();
            }

            return settings;
        }

        // Gets the path to the JSON file next to the binaries
        private static string GetJsonFilePath()
        {
            string exeDir = AppContext.BaseDirectory;
            return Path.Combine(exeDir, "settings.json");
        }

        // Checks if the settings file exists
        public static bool SettingsFileExists()
        {
            return File.Exists(GetJsonFilePath());
        }
    }
}
