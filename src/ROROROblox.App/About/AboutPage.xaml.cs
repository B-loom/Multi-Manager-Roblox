using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ROROROblox.App.Logging;

namespace ROROROblox.App.About;

/// <summary>
/// The About destination, hosted by the shell (F-013 — formerly <c>AboutWindow</c>). The shell's
/// title bar says "About"; the hero inside says who the product is, which is the same division of
/// labor the window had.
/// </summary>
internal partial class AboutPage : UserControl
{
    private const string RepoUrl = "https://github.com/B-loom/Multi-Manager-Roblox";
    private const string IssuesUrl = "https://github.com/B-loom/Multi-Manager-Roblox/issues";

    // Easter egg: clicking the version number 6 OR 7 times reveals "Koii 4 eva". The exact target
    // is randomized per construction so the click count is non-deterministic. A page is constructed
    // once per shell lifetime, so "per construction" means per shell — the egg survives navigating
    // away and back, which suits an egg.
    //
    // The counting moved to EasterEggCounter (2026-10-06) so it can be tested: the target used to be
    // Random.Shared at a field initialiser here, which no test could pin, leaving the exclusive
    // upper bound in Next(6, 8) — the whole of the 6-7 joke — guarded by nothing. What stays here is
    // the half a test cannot see: the fade and the visibility flip.
    private readonly EasterEggCounter _egg = EasterEggCounter.CreateRandom();

    public AboutPage()
    {
        InitializeComponent();
        var version = Distribution.AppVersion.Display(new Distribution.Win32DistributionMode());
        VersionText.Text = $"v{version}";

        // One source of truth (F-112): the same table the windows build their bindings from.
        var french = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr";
        var fr = new Dictionary<string, string>
        {
            ["Add account"] = "Ajouter un compte", ["Launch multiple"] = "Lancer plusieurs",
            ["Squad Launch"] = "Lancement en équipe", ["Filter accounts"] = "Filtrer les comptes",
            ["Library"] = "Bibliothèque", ["Settings"] = "Paramètres", ["History"] = "Historique",
            ["Diagnostics"] = "Diagnostics", ["Plugins"] = "Plugins",
            ["Keyboard shortcuts (this list)"] = "Raccourcis clavier (cette liste)",
        };
        ShortcutsList.ItemsSource = Input.KeyboardVocabulary.Shortcuts
            .Select(s => new { s.GestureText, Label = french && fr.TryGetValue(s.Label, out var t) ? t : s.Label })
            .ToList();
    }

    private void OnSiteClick(object sender, System.Windows.RoutedEventArgs e)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://b-loom.github.io/Multi-Manager-Roblox/") { UseShellExecute = true }); }
        catch { }
    }

    private void OnRepoClick(object sender, RoutedEventArgs e) => OpenUrl(RepoUrl);
    private void OnIssuesClick(object sender, RoutedEventArgs e) => OpenUrl(IssuesUrl);

    private void OnLicenseClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = AppLogging.LogDirectory,
                UseShellExecute = true,
                Verb = "open",
            });
        }
        catch
        {
            // Best-effort.
        }
    }

    /// <summary>
    /// F-039's remaining half. The tour is the only documentation of six unlabelled row
    /// affordances, and it is shown exactly once — before the user has any accounts, i.e. before
    /// any of those affordances exist to look at. Wave 3 gave it a door in the Tools menu; this is
    /// the one the row also asked for, in the page a person opens when they want to know what
    /// something is. Owned by the shell window, so it opens above the surface that launched it.
    /// </summary>
    private void OnWelcomeTourClick(object sender, RoutedEventArgs e)
        => WelcomeWindow.ShowTour(Window.GetWindow(this));

    private void OnVersionClicked(object sender, MouseButtonEventArgs e)
    {
        if (!_egg.Click()) return;

        EasterEggMark.Visibility = Visibility.Visible;
        var fade = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(380),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        EasterEggMark.BeginAnimation(OpacityProperty, fade);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        catch
        {
            // Best-effort.
        }
    }
}
