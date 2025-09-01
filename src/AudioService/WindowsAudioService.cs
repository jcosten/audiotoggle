using System;
using System.Collections.Generic;
using System.Linq;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi;

namespace AudioToggle
{
    public class WindowsAudioService : IAudioService
    {
        private static readonly Lazy<CoreAudioController> _controller = new Lazy<CoreAudioController>(() => new CoreAudioController());
        private static CoreAudioController Controller => _controller.Value;
        
        // Cache devices to avoid repeated enumeration
        private List<CoreAudioDevice> _cachedPlaybackDevices;
        private List<CoreAudioDevice> _cachedInputDevices;
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheValidTime = TimeSpan.FromSeconds(5); // Cache for 5 seconds

        private List<CoreAudioDevice> GetCachedActivePlaybackDevices()
        {
            if (_cachedPlaybackDevices == null || DateTime.Now - _lastCacheUpdate > _cacheValidTime)
            {
                _cachedPlaybackDevices = Controller.GetPlaybackDevices()
                    .Where(d => d.State == DeviceState.Active)
                    .ToList();
                _lastCacheUpdate = DateTime.Now;
            }
            return _cachedPlaybackDevices;
        }

        private List<CoreAudioDevice> GetCachedActiveInputDevices()
        {
            if (_cachedInputDevices == null || DateTime.Now - _lastCacheUpdate > _cacheValidTime)
            {
                _cachedInputDevices = Controller.GetCaptureDevices()
                    .Where(d => d.State == DeviceState.Active)
                    .ToList();
                _lastCacheUpdate = DateTime.Now;
            }
            return _cachedInputDevices;
        }

        public (string Name, string ID)? GetDefaultPlaybackDevice()
        {
            var device = Controller.DefaultPlaybackDevice;
            if (device != null)
            {
                return (device.FullName, device.Id.ToString());
            }
            return null;
        }

        public List<string> GetAudioDeviceNames()
        {
            return GetCachedActivePlaybackDevices()
                .Select(device => device.FullName)
                .ToList();
        }

        // method to get device by friendly name
        public (string Name, string ID)? GetDeviceByFriendlyName(string friendlyName)
        {
            var device = GetCachedActivePlaybackDevices()
                .FirstOrDefault(d => d.FullName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));
            
            if (device != null)
            {
                return (device.FullName, device.Id.ToString());
            }
            return null;
        }

        public bool SetDefaultPlaybackDevice(string friendlyName)
        {
            try
            {
                // Find device directly from cache instead of multiple lookups
                var device = GetCachedActivePlaybackDevices()
                    .FirstOrDefault(d => d.FullName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));

                if (device == null)
                {
                    // If not in cache, refresh and try again
                    InvalidateCache();
                    device = GetCachedActivePlaybackDevices()
                        .FirstOrDefault(d => d.FullName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));
                }

                if (device != null)
                {
                    // Use async method for better performance and return immediately
                    _ = device.SetAsDefaultAsync();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting default playback device: {ex.Message}");
                return false;
            }
        }

        // Input device methods
        public (string Name, string ID)? GetDefaultInputDevice()
        {
            var device = Controller.DefaultCaptureDevice;
            if (device != null)
            {
                return (device.FullName, device.Id.ToString());
            }
            return null;
        }

        public List<string> GetInputDeviceNames()
        {
            return GetCachedActiveInputDevices()
                .Select(device => device.FullName)
                .ToList();
        }

        public (string Name, string ID)? GetInputDeviceByFriendlyName(string friendlyName)
        {
            var device = GetCachedActiveInputDevices()
                .FirstOrDefault(d => d.FullName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));
            
            if (device != null)
            {
                return (device.FullName, device.Id.ToString());
            }
            return null;
        }

        public bool SetDefaultInputDevice(string friendlyName)
        {
            try
            {
                // Find device directly from cache instead of multiple lookups
                var device = GetCachedActiveInputDevices()
                    .FirstOrDefault(d => d.FullName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));

                if (device == null)
                {
                    // If not in cache, refresh and try again
                    InvalidateCache();
                    device = GetCachedActiveInputDevices()
                        .FirstOrDefault(d => d.FullName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));
                }

                if (device != null)
                {
                    // Use async method for better performance and return immediately
                    _ = device.SetAsDefaultAsync();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting default input device: {ex.Message}");
                return false;
            }
        }

        public void InvalidateCache()
        {
            _cachedPlaybackDevices = null;
            _cachedInputDevices = null;
            _lastCacheUpdate = DateTime.MinValue;
        }
    }
}
