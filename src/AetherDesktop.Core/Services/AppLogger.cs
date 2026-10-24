namespace AetherDesktop.Core.Services;

public static class AppLogger
{
    private static readonly object SyncRoot = new();

    public static string LogDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AetherDesktop",
        "logs");

    public static string LogPath => Path.Combine(LogDirectory, "aetherdesktop.log");

    public static void Log(string component, string message, string level = "Info")
    {
        lock (SyncRoot)
        {
            Directory.CreateDirectory(LogDirectory);
            var line = $"[{DateTimeOffset.UtcNow:O}] [{level}] [{component}] {message}";
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }
    }

    public static void LogException(string component, Exception exception)
    {
        Log(component, $"{exception.GetType().Name}: {exception.Message}", "Error");
    }
}
