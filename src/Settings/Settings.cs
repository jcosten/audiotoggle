using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AudioToggle
{
    /// <summary>
    /// Represents the application settings data model
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Hotkey for switching output devices (e.g., "Ctrl+Shift+F1")
        /// </summary>
        [JsonPropertyName("outputHotkey")]
        public string OutputHotkey { get; set; } = "Ctrl+Shift+F1";

        /// <summary>
        /// Hotkey for switching input devices (e.g., "Ctrl+Shift+F2")
        /// </summary>
        [JsonPropertyName("inputHotkey")]
        public string InputHotkey { get; set; } = "Ctrl+Shift+F2";

        /// <summary>
        /// List of enabled output device names
        /// </summary>
        [JsonPropertyName("enabledDevices")]
        public List<string> EnabledDevices { get; set; } = new List<string>();

        /// <summary>
        /// List of enabled input device names
        /// </summary>
        [JsonPropertyName("enabledInputDevices")]
        public List<string> EnabledInputDevices { get; set; } = new List<string>();

        /// <summary>
        /// Whether the application should start with Windows
        /// </summary>
        [JsonPropertyName("startWithWindows")]
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// Whether to show notifications when switching devices
        /// </summary>
        [JsonPropertyName("showNotifications")]
        public bool ShowNotifications { get; set; } = true;

        /// <summary>
        /// Whether automatic updates are enabled
        /// </summary>
        [JsonPropertyName("autoUpdateEnabled")]
        public bool AutoUpdateEnabled { get; set; } = true;

        /// <summary>
        /// The default playback device name
        /// </summary>
        [JsonPropertyName("defaultPlayback")]
        public string DefaultPlayback { get; set; } = string.Empty;

        /// <summary>
        /// The default input device name
        /// </summary>
        [JsonPropertyName("defaultInput")]
        public string DefaultInput { get; set; } = string.Empty;

        /// <summary>
        /// The date of the last update check (format: yyyy-MM-dd)
        /// </summary>
        [JsonPropertyName("lastUpdateCheck")]
        public string LastUpdateCheck { get; set; } = string.Empty;

        /// <summary>
        /// Creates a deep copy of the settings
        /// </summary>
        public Settings Clone()
        {
            return new Settings
            {
                OutputHotkey = this.OutputHotkey,
                InputHotkey = this.InputHotkey,
                EnabledDevices = new List<string>(this.EnabledDevices),
                EnabledInputDevices = new List<string>(this.EnabledInputDevices),
                StartWithWindows = this.StartWithWindows,
                ShowNotifications = this.ShowNotifications,
                AutoUpdateEnabled = this.AutoUpdateEnabled,
                DefaultPlayback = this.DefaultPlayback,
                DefaultInput = this.DefaultInput,
                LastUpdateCheck = this.LastUpdateCheck
            };
        }

        /// <summary>
        /// Validates the settings and returns any validation errors
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(OutputHotkey))
            {
                errors.Add("Output hotkey cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(InputHotkey))
            {
                errors.Add("Input hotkey cannot be empty");
            }

            if (EnabledDevices == null)
            {
                errors.Add("Enabled devices list cannot be null");
            }

            if (EnabledInputDevices == null)
            {
                errors.Add("Enabled input devices list cannot be null");
            }

            return errors;
        }
    }
}
