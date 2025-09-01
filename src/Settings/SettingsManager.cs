using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioToggle
{
    /// <summary>
    /// Manages application settings with change tracking and validation
    /// </summary>
    public class SettingsManager : INotifyPropertyChanged
    {
        private Settings _currentSettings;
        private bool _hasUnsavedChanges;

        /// <summary>
        /// Event raised when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets whether there are unsaved changes
        /// </summary>
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            private set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the current settings
        /// </summary>
        public Settings CurrentSettings => _currentSettings;

        /// <summary>
        /// Initializes a new instance of the SettingsManager
        /// </summary>
        public SettingsManager()
        {
            _currentSettings = PersistService.LoadSettings();
            _hasUnsavedChanges = false;
        }

        /// <summary>
        /// Gets the output hotkey
        /// </summary>
        public string OutputHotkey
        {
            get => _currentSettings.OutputHotkey;
            set
            {
                if (_currentSettings.OutputHotkey != value)
                {
                    _currentSettings.OutputHotkey = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the input hotkey
        /// </summary>
        public string InputHotkey
        {
            get => _currentSettings.InputHotkey;
            set
            {
                if (_currentSettings.InputHotkey != value)
                {
                    _currentSettings.InputHotkey = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to start with Windows
        /// </summary>
        public bool StartWithWindows
        {
            get => _currentSettings.StartWithWindows;
            set
            {
                if (_currentSettings.StartWithWindows != value)
                {
                    _currentSettings.StartWithWindows = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show notifications
        /// </summary>
        public bool ShowNotifications
        {
            get => _currentSettings.ShowNotifications;
            set
            {
                if (_currentSettings.ShowNotifications != value)
                {
                    _currentSettings.ShowNotifications = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether auto updates are enabled
        /// </summary>
        public bool AutoUpdateEnabled
        {
            get => _currentSettings.AutoUpdateEnabled;
            set
            {
                if (_currentSettings.AutoUpdateEnabled != value)
                {
                    _currentSettings.AutoUpdateEnabled = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the default playback device
        /// </summary>
        public string DefaultPlayback
        {
            get => _currentSettings.DefaultPlayback;
            set
            {
                if (_currentSettings.DefaultPlayback != value)
                {
                    _currentSettings.DefaultPlayback = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the default input device
        /// </summary>
        public string DefaultInput
        {
            get => _currentSettings.DefaultInput;
            set
            {
                if (_currentSettings.DefaultInput != value)
                {
                    _currentSettings.DefaultInput = value;
                    HasUnsavedChanges = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the enabled devices list
        /// </summary>
        public System.Collections.Generic.List<string> EnabledDevices => _currentSettings.EnabledDevices;

        /// <summary>
        /// Gets the enabled input devices list
        /// </summary>
        public System.Collections.Generic.List<string> EnabledInputDevices => _currentSettings.EnabledInputDevices;

        /// <summary>
        /// Updates the last update check date to today
        /// </summary>
        public void UpdateLastUpdateCheck()
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            if (_currentSettings.LastUpdateCheck != today)
            {
                _currentSettings.LastUpdateCheck = today;
                HasUnsavedChanges = true;
                OnPropertyChanged(nameof(LastUpdateCheck));
            }
        }

        /// <summary>
        /// Gets the last update check date
        /// </summary>
        public string LastUpdateCheck => _currentSettings.LastUpdateCheck;

        /// <summary>
        /// Saves the current settings to disk
        /// </summary>
        public void Save()
        {
            // Validate settings before saving
            var validationErrors = _currentSettings.Validate();
            if (validationErrors.Count > 0)
            {
                throw new InvalidOperationException($"Settings validation failed: {string.Join(", ", validationErrors)}");
            }

            PersistService.SaveSettings(_currentSettings);
            HasUnsavedChanges = false;
        }

        /// <summary>
        /// Reloads settings from disk, discarding any unsaved changes
        /// </summary>
        public void Reload()
        {
            _currentSettings = PersistService.LoadSettings();
            HasUnsavedChanges = false;
            OnPropertyChanged(string.Empty); // Notify all properties changed
        }

        /// <summary>
        /// Resets all settings to their default values
        /// </summary>
        public void ResetToDefaults()
        {
            _currentSettings = new Settings();
            HasUnsavedChanges = true;
            OnPropertyChanged(string.Empty); // Notify all properties changed
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
