using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using Velopack;
using Velopack.Sources;

namespace ROROROblox.App.Updates;

/// <summary>
/// Multi Manager Roblox: at startup, asks GitHub for the latest release of the app's repository.
/// When it is newer than the running version, the user is told in the app and offered the update:
/// an installed (Velopack) copy downloads it and restarts on the new version; any other copy
/// (a build from source, a portable folder) opens the release page to download it there.
/// Every failure is silent: no network or no release simply means no prompt.
/// </summary>
internal static class ReleaseNotifier
{
    internal const string RepoUrl = "https://github.com/B-loom/Multi-Manager-Roblox";
    private const string LatestApi = "https://api.github.com/repos/B-loom/Multi-Manager-Roblox/releases/latest";

    public static async Task CheckAndPromptAsync(Window? owner)
    {
        try
        {
            var current = typeof(ReleaseNotifier).Assembly.GetName().Version ?? new Version(0, 0);
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MultiManagerRoblox", current.ToString(3)));
            http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

            using var resp = await http.GetAsync(LatestApi).ConfigureAwait(true);
            if (!resp.IsSuccessStatusCode) return; // 404 = no release published yet
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(true));
            var tag = doc.RootElement.GetProperty("tag_name").GetString() ?? "";
            var pageUrl = doc.RootElement.TryGetProperty("html_url", out var u) ? u.GetString() : null;
            if (!Version.TryParse(tag.TrimStart('v', 'V'), out var latest)) return;
            if (Normalize(latest) <= Normalize(current)) return;

            var fr = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr";
            var shown = latest.ToString(3);
            var text = fr
                ? $"Une nouvelle version de Multi Manager Roblox est disponible : {shown} (vous avez {current.ToString(3)}).\n\nLa télécharger maintenant ?"
                : $"A new version of Multi Manager Roblox is available: {shown} (you have {current.ToString(3)}).\n\nDownload it now?";
            var answer = owner is not null
                ? MessageBox.Show(owner, text, fr ? "Mise à jour disponible" : "Update available", MessageBoxButton.YesNo, MessageBoxImage.Information)
                : MessageBox.Show(text, fr ? "Mise à jour disponible" : "Update available", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (answer != MessageBoxResult.Yes) return;

            var manager = new UpdateManager(new GithubSource(RepoUrl, accessToken: null, prerelease: false));
            if (manager.IsInstalled)
            {
                var update = await manager.CheckForUpdatesAsync().ConfigureAwait(true);
                if (update is not null)
                {
                    await manager.DownloadUpdatesAsync(update).ConfigureAwait(true);
                    manager.ApplyUpdatesAndRestart(update.TargetFullRelease);
                    return;
                }
            }

            Process.Start(new ProcessStartInfo(pageUrl ?? RepoUrl + "/releases/latest") { UseShellExecute = true });
        }
        catch
        {
            // Offline, rate-limited or malformed: no prompt this time.
        }
    }

    private static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(v.Build, 0));
}
