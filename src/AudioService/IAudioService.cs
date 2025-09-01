using System.Collections.Generic;

namespace AudioToggle
{
    public interface IAudioService
    {
        // Output/Playback device methods
        List<string> GetAudioDeviceNames();
        (string Name, string ID)? GetDeviceByFriendlyName(string friendlyName);
        bool SetDefaultPlaybackDevice(string friendlyName);
        (string Name, string ID)? GetDefaultPlaybackDevice();
        void InvalidateCache();

        // Input device methods
        List<string> GetInputDeviceNames();
        (string Name, string ID)? GetInputDeviceByFriendlyName(string friendlyName);
        bool SetDefaultInputDevice(string friendlyName);
        (string Name, string ID)? GetDefaultInputDevice();
    }
}
