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

        [UnconditionalSuppressMessage("Trim", "IL2026:Using member 'System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)' which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming application code", Justification = "Types are known at compile time and preserved by JsonSerializable attributes")]
        public List<string> GetEnabledDevicesForCycling()
        {
            var enabledDevicesJson = PersistService.GetString("enabledDevices", "[]");
            try
            {
                var enabledDevices = System.Text.Json.JsonSerializer.Deserialize(enabledDevicesJson, typeof(List<string>), AudioToggleJsonContext.Default) as List<string> ?? new List<string>();
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
    }

}
