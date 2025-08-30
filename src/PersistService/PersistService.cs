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
        [UnconditionalSuppressMessage("Trim", "IL2026:Using member 'System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)' which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming application code", Justification = "Types are known at compile time and preserved by JsonSerializable attributes")]
        public static void StoreString(string key, string value)
        {
            string filePath = GetJsonFilePath();
            var dict = File.Exists(filePath)
                ? JsonSerializer.Deserialize(File.ReadAllText(filePath), typeof(Dictionary<string, string>), AudioToggleJsonContext.Default) as Dictionary<string, string>
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
        [UnconditionalSuppressMessage("Trim", "IL2026:Using member 'System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)' which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming application code", Justification = "Types are known at compile time and preserved by JsonSerializable attributes")]
        public static string GetString(string key)
        {
            string filePath = GetJsonFilePath();
            if (!File.Exists(filePath)) return null;
            var dict = JsonSerializer.Deserialize(File.ReadAllText(filePath), typeof(Dictionary<string, string>), AudioToggleJsonContext.Default) as Dictionary<string, string>;
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

        // Gets the path to the JSON file next to the binaries
        private static string GetJsonFilePath()
        {
            string exeDir = AppContext.BaseDirectory;
            return Path.Combine(exeDir, "settings.json");
        }
    }
}
