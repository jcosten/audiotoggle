using System.Collections.Generic;

namespace AudioToggle
{
    public interface IAudioService
    {
        // Output/Playback device methods
        List<string> GetOutputDeviceNames();
        (string Name, string ID)? GetOutputDeviceByFriendlyName(string friendlyName);
        bool SetDefaultOutputDevice(string friendlyName);
        (string Name, string ID)? GetDefaultOutputDevice();


        // Input device methods
        List<string> GetInputDeviceNames();
        (string Name, string ID)? GetInputDeviceByFriendlyName(string friendlyName);
        bool SetDefaultInputDevice(string friendlyName);
        (string Name, string ID)? GetDefaultInputDevice();

        void InvalidateCache();
    }
}
