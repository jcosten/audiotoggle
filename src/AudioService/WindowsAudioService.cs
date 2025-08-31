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
        
        // Optimized cache - store only essential device info to reduce memory
        private List<(string Name, string Id, bool IsDefault)> _deviceInfoCache;
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheValidTime = TimeSpan.FromSeconds(10); // Increased cache time to reduce updates

        private List<(string Name, string Id, bool IsDefault)> GetCachedDeviceInfo()
        {
            if (_deviceInfoCache == null || DateTime.Now - _lastCacheUpdate > _cacheValidTime)
            {
                var devices = Controller.GetPlaybackDevices()
                    .Where(d => d.State == DeviceState.Active)
                    .Select(d => (d.FullName, d.Id.ToString(), d.IsDefaultDevice))
                    .ToList();
                
                _deviceInfoCache = devices;
                _lastCacheUpdate = DateTime.Now;
            }
            return _deviceInfoCache;
        }

        private CoreAudioDevice GetDeviceById(string deviceId)
        {
            // Only get the specific device when needed, don't cache full objects
            return Controller.GetDevice(Guid.Parse(deviceId));
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
            return GetCachedDeviceInfo()
                .Select(deviceInfo => deviceInfo.Name)
                .ToList();
        }

        // method to get device by friendly name
        public (string Name, string ID)? GetDeviceByFriendlyName(string friendlyName)
        {
            var deviceInfo = GetCachedDeviceInfo()
                .FirstOrDefault(d => d.Name.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));
            
            if (deviceInfo != default)
            {
                return (deviceInfo.Name, deviceInfo.Id);
            }
            return null;
        }

        public bool SetDefaultPlaybackDevice(string friendlyName)
        {
            try
            {
                // Find device info from cache first
                var deviceInfo = GetCachedDeviceInfo()
                    .FirstOrDefault(d => d.Name.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));

                if (deviceInfo == default)
                {
                    // If not in cache, refresh and try again
                    InvalidateCache();
                    deviceInfo = GetCachedDeviceInfo()
                        .FirstOrDefault(d => d.Name.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));
                }

                if (deviceInfo != default)
                {
                    // Get the actual device only when we need to set it as default
                    var device = GetDeviceById(deviceInfo.Id);
                    if (device != null)
                    {
                        // Use async method for better performance and return immediately
                        _ = device.SetAsDefaultAsync();
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    Console.WriteLine($"Error setting default playback device: {ex.Message}");
                return false;
            }
        }

        public void InvalidateCache()
        {
            _deviceInfoCache = null;
            _lastCacheUpdate = DateTime.MinValue;
        }
    }
}
