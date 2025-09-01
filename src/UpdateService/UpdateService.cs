using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;

namespace AudioToggle
{
    public class UpdateService : IUpdateService
    {
        private readonly HttpClient _httpClient;
        private const string GitHubApiUrl = "https://api.github.com/repos/jcosten/audiotoggle/releases/latest";

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AudioToggle-UpdateChecker");
        }

        public string GetCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(GitHubApiUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubRelease>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (release == null) return null;

                System.Diagnostics.Debug.WriteLine($"Found release: {release.TagName}, Assets count: {release.Assets?.Length ?? 0}");

                // If no assets available, return null
                if (release.Assets == null || release.Assets.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No assets found in release");
                    return null;
                }

                // Find the Windows asset (ZIP file for Windows)
                var windowsAsset = release.Assets.FirstOrDefault(a =>
                {
                    System.Diagnostics.Debug.WriteLine($"Checking asset: {a.Name}, DownloadUrl: {a.BrowserDownloadUrl ?? a.Browser_download_url}");
                    return a.Name.Contains("AudioToggle_Windows", StringComparison.OrdinalIgnoreCase) &&
                           a.Name.Contains(".zip", StringComparison.OrdinalIgnoreCase);
                });

                // If no specific Windows asset found, try to find any ZIP file
                if (windowsAsset == null)
                {
                    windowsAsset = release.Assets.FirstOrDefault(a =>
                        a.Name.Contains(".zip", StringComparison.OrdinalIgnoreCase));
                    if (windowsAsset != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found alternative ZIP asset: {windowsAsset.Name}");
                    }
                }

                var downloadUrl = windowsAsset?.BrowserDownloadUrl ?? windowsAsset?.Browser_download_url;
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    // Try alternative field names that might be used by GitHub API
                    if (windowsAsset != null)
                    {
                        // Check if there are other possible download URL fields
                        var assetJson = JsonSerializer.Serialize(windowsAsset);
                        System.Diagnostics.Debug.WriteLine($"Asset JSON: {assetJson}");
                    }

                    // Fallback to zipball if no download URL available
                    downloadUrl = release.ZipballUrl;
                    System.Diagnostics.Debug.WriteLine("Using fallback download URL: " + downloadUrl);
                }

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    System.Diagnostics.Debug.WriteLine("No download URL available for this release");
                    return null;
                }

                return new UpdateInfo
                {
                    Version = release.TagName.TrimStart('v'),
                    DownloadUrl = downloadUrl,
                    ReleaseNotes = release.Body,
                    PublishedAt = release.PublishedAt,
                    IsPrerelease = release.Prerelease
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to check for updates: {ex.Message}");
                return null;
            }
        }

        public bool IsUpdateAvailable(UpdateInfo updateInfo)
        {
            if (updateInfo == null) return false;

            var currentVersion = GetCurrentVersion();
            return IsVersionNewer(updateInfo.Version, currentVersion);
        }

        public async Task DownloadUpdateAsync(UpdateInfo updateInfo, string downloadPath)
        {
            if (updateInfo == null || string.IsNullOrEmpty(updateInfo.DownloadUrl))
                throw new ArgumentException("Invalid update info or download URL");

            try
            {
                using var response = await _httpClient.GetAsync(updateInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = System.IO.File.Create(downloadPath);

                await stream.CopyToAsync(fileStream);

                System.Diagnostics.Debug.WriteLine($"Downloaded update to: {downloadPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to download update: {ex.Message}");
                throw;
            }
        }

        private bool IsVersionNewer(string newVersion, string currentVersion)
        {
            try
            {
                var newVer = Version.Parse(newVersion);
                var currentVer = Version.Parse(currentVersion);
                return newVer > currentVer;
            }
            catch
            {
                // If parsing fails, do string comparison
                return string.Compare(newVersion, currentVersion, StringComparison.OrdinalIgnoreCase) > 0;
            }
        }

        private class GitHubRelease
        {
            public string TagName { get; set; }
            public string Name { get; set; }
            public string Body { get; set; }
            public bool Prerelease { get; set; }
            public DateTime PublishedAt { get; set; }
            public string ZipballUrl { get; set; }
            public GitHubAsset[] Assets { get; set; }
        }

        private class GitHubAsset
        {
            public string Name { get; set; }
            public string BrowserDownloadUrl { get; set; }
            public string Browser_download_url { get; set; } // Alternative field name
            public long Size { get; set; }
        }
    }
}
