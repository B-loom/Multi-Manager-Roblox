using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ROROROblox.App.Localization;

/// <summary>
/// Multi Manager Roblox: the language choice shown on the very first launch (no language saved
/// yet). Runs in Program.Main before the App exists, so it is built in code with literal colours
/// and depends on no application resources. The pick is saved, so it never shows again; the
/// Settings page can change it later.
/// </summary>
internal static class FirstRunLanguagePicker
{
    public static void ShowIfNeeded()
    {
        try
        {
            if (Core.AppSettings.ReadUiLanguageFast() is not null) return;

            _ = System.IO.Packaging.PackUriHelper.UriSchemePack; // registers pack:// before the App exists
            string? picked = null;
            var bg = new SolidColorBrush(Color.FromRgb(0x0B, 0x07, 0x20));
            var win = new Window
            {
                Title = "Multi Manager Roblox",
                Width = 420, SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = bg,
            };
            try { win.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/ROROROblox.App;component/Tray/Resources/logo.png")); } catch { }

            var panel = new StackPanel { Margin = new Thickness(28, 24, 28, 26) };
            try
            {
                panel.Children.Add(new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/ROROROblox.App;component/Tray/Resources/logo.png")),
                    Width = 96, Height = 96, Margin = new Thickness(0, 0, 0, 10),
                });
            }
            catch { }
            panel.Children.Add(new TextBlock
            {
                Text = "Multi Manager Roblox", FontSize = 22, FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new LinearGradientBrush(Color.FromRgb(0xB6, 0x9C, 0xFF), Color.FromRgb(0xFF, 0x8F, 0xE0), 0),
            });
            panel.Children.Add(new TextBlock
            {
                Text = "Choose your language  ·  Choisissez votre langue", FontSize = 13,
                Margin = new Thickness(0, 6, 0, 18), HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(0xA5, 0x9C, 0xC8)),
            });

            Button Choice(string label, string culture, bool primary)
            {
                var b = new Button
                {
                    Content = label, Height = 44, FontSize = 15, FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 10), Cursor = System.Windows.Input.Cursors.Hand,
                    Foreground = primary ? bg : Brushes.White,
                    Background = primary
                        ? new LinearGradientBrush(Color.FromRgb(0xB6, 0x9C, 0xFF), Color.FromRgb(0xFF, 0x8F, 0xE0), 0)
                        : new SolidColorBrush(Color.FromRgb(0x2A, 0x1F, 0x55)),
                    BorderThickness = new Thickness(0),
                };
                b.Click += (_, _) => { picked = culture; win.Close(); };
                return b;
            }

            var osFrench = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr";
            panel.Children.Add(Choice("English", "en", !osFrench));
            panel.Children.Add(Choice("Français", "fr", osFrench));
            win.Content = panel;
            win.ShowDialog();

            // Closing the window without a choice keeps the system language and asks again next time.
            if (picked is null) return;
            new Core.AppSettings().SetUiLanguageAsync(picked).GetAwaiter().GetResult();
        }
        catch
        {
            // A failed picker must never stop the app starting; the system language applies.
        }
    }
}
