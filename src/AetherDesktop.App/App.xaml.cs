using System.Windows;
using AetherDesktop.Core.Services;

namespace AetherDesktop.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppLogger.Log(nameof(App), "Aether Desktop started.", "Info");

        if (Current.Resources.MergedDictionaries.Count == 0)
        {
            AppLogger.Log(nameof(App), "WPF resources did not initialize correctly.", "Warning");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppLogger.Log(nameof(App), "Aether Desktop shutting down.", "Info");
        base.OnExit(e);
    }
}
