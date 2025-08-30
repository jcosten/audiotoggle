using System.Collections.Generic;

namespace AudioToggle
{
    public interface IAudioService
    {
        List<string> GetAudioDeviceNames();
        (string Name, string ID)? GetDeviceByFriendlyName(string friendlyName);
        bool SetDefaultPlaybackDevice(string friendlyName);
        (string Name, string ID)? GetDefaultPlaybackDevice();
        void InvalidateCache();
    }
}
