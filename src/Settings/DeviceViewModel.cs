using System;
using System.ComponentModel;

namespace AudioToggle
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private bool _isEnabled;
        private bool _isDefault;
        
        public string Name { get; set; }
        
        public bool IsDefault 
        { 
            get => _isDefault;
            set
            {
                if (_isDefault != value)
                {
                    _isDefault = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDefault)));
                }
            }
        }
        
        public bool IsEnabled 
        { 
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
                    EnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler EnabledChanged;
    }
}
