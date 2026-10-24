using System.Text.Json;

namespace AetherDesktop.Core.Configuration;

public class AppSettings
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Dark;
    public string Language { get; set; } = "en-US";
    public bool StartWithWindows { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool AutoPauseOnFullScreen { get; set; } = true;
    public bool RequireExplicitWallpaperExit { get; set; } = false;
    public double Volume { get; set; } = 0.75;
    public List<string> LibraryPaths { get; set; } = new();
    public bool UseDarkMode { get; set; } = true;
}

public static class AppSettingsStore
{
    public static string DefaultSettingsPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AetherDesktop",
        "settings.json");

    public static AppSettings Load(string? path = null)
    {
        var settingsPath = string.IsNullOrWhiteSpace(path) ? DefaultSettingsPath : path;

        if (!File.Exists(settingsPath))
        {
            return new AppSettings();
        }

        var json = File.ReadAllText(settingsPath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AppSettings();
        }

        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public static void Save(AppSettings settings, string? path = null)
    {
        var settingsPath = string.IsNullOrWhiteSpace(path) ? DefaultSettingsPath : path;
        var directory = Path.GetDirectoryName(settingsPath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(settingsPath, json);
    }
}
