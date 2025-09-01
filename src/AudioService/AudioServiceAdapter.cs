using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

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

        public List<string> GetAudioDeviceNames() => impl.GetAudioDeviceNames();

        public (string Name, string ID)? GetDeviceByFriendlyName(string friendlyName)
            => impl.GetDeviceByFriendlyName(friendlyName);

        public bool SetDefaultPlaybackDevice(string friendlyName)
            => impl.SetDefaultPlaybackDevice(friendlyName);

        public (string Name, string ID)? GetDefaultPlaybackDevice()
            => impl.GetDefaultPlaybackDevice();

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

        public List<string> GetEnabledDevicesForCycling()
        {
            var enabledDevicesJson = PersistService.GetString("enabledDevices", "[]");
            try
            {
                var enabledDevices = System.Text.Json.JsonSerializer.Deserialize(enabledDevicesJson, typeof(List<string>)) as List<string> ?? new List<string>();
                // Filter to only include devices that actually exist
                var allDevices = GetAudioDeviceNames();
                return enabledDevices.Where(name => allDevices.Contains(name)).ToList();
            }
            catch
            {
                // Fallback to all devices if there's an issue
                return GetAudioDeviceNames();
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
