using System.Windows;
using ExcursionTextbook.Services;

namespace ExcursionTextbook;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var progress = new ProgressService();
        if (progress.Theme == "Dark")
        {
            var existing = Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Theme") == true);
            if (existing != null) Resources.MergedDictionaries.Remove(existing);

            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            });
        }
    }
}
