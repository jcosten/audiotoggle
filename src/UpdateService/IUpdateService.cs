using System;
using System.Threading.Tasks;

namespace AudioToggle
{
    public interface IUpdateService
    {
        Task<UpdateInfo> CheckForUpdatesAsync();
        Task DownloadUpdateAsync(UpdateInfo updateInfo, string downloadPath);
        string GetCurrentVersion();
        bool IsUpdateAvailable(UpdateInfo updateInfo);
    }

    public class UpdateInfo
    {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool IsPrerelease { get; set; }
    }
}
