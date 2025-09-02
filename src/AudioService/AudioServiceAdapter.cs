using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AudioToggle
{
    // Adapter that selects a platform-specific implementation at runtime
    public class AudioServiceAdapter : IAudioService
    {
        private readonly IAudioService impl;

        public AudioServiceAdapter()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                impl = new WindowsAudioService();
            else
                throw new PlatformNotSupportedException("AudioService is only supported on Windows.");
        }

        public List<string> GetOutputDeviceNames() => impl.GetOutputDeviceNames();

        public (string Name, string ID)? GetOutputDeviceByFriendlyName(string friendlyName)
            => impl.GetOutputDeviceByFriendlyName(friendlyName);

        public bool SetDefaultOutputDevice(string friendlyName)
            => impl.SetDefaultOutputDevice(friendlyName);

        public (string Name, string ID)? GetDefaultOutputDevice()
            => impl.GetDefaultOutputDevice();

        public void InvalidateCache()
            => impl.InvalidateCache();

        // Input device methods
        public List<string> GetInputDeviceNames() => impl.GetInputDeviceNames();

        public (string Name, string ID)? GetInputDeviceByFriendlyName(string friendlyName)
            => impl.GetInputDeviceByFriendlyName(friendlyName);

        public bool SetDefaultInputDevice(string friendlyName)
            => impl.SetDefaultInputDevice(friendlyName);

        public (string Name, string ID)? GetDefaultInputDevice()
            => impl.GetDefaultInputDevice();

        public List<string> GetEnabledOutputDevicesForCycling()
        {
            var enabledDevicesJson = PersistService.GetString("enabledDevices", "[]");
            try
            {
                var enabledDevices = System.Text.Json.JsonSerializer.Deserialize(enabledDevicesJson, typeof(List<string>)) as List<string> ?? new List<string>();
                // Filter to only include devices that actually exist
                var allDevices = GetOutputDeviceNames();
                return enabledDevices.Where(name => allDevices.Contains(name)).ToList();
            }
            catch
            {
                // Fallback to all devices if there's an issue
                return GetOutputDeviceNames();
            }
        }

        public List<string> GetEnabledInputDevicesForCycling()
        {
            var enabledDevicesJson = PersistService.GetString("enabledInputDevices", "[]");
            try
            {
                var enabledDevices = System.Text.Json.JsonSerializer.Deserialize(enabledDevicesJson, typeof(List<string>)) as List<string> ?? new List<string>();
                // Filter to only include devices that actually exist
                var allDevices = GetInputDeviceNames();
                return enabledDevices.Where(name => allDevices.Contains(name)).ToList();
            }
            catch
            {
                // Fallback to all devices if there's an issue
                return GetInputDeviceNames();
            }
        }
    }

}
