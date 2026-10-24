using System;
using System.Collections.Generic;
using System.IO;
using AetherDesktop.Core.Configuration;
using AetherDesktop.Core.Models;
using Xunit;

namespace AetherDesktop.Core.Tests;

public class AppSettingsStoreTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsSettings()
    {
        var path = Path.Combine(Path.GetTempPath(), $"aether-settings-{Guid.NewGuid():N}.json");
        var settings = new AppSettings
        {
            ThemeMode = ThemeMode.Dark,
            Language = "ru-RU",
            Volume = 0.45,
            MinimizeToTray = true,
            StartWithWindows = false,
            LibraryPaths = new List<string> { "C:\\Wallpapers", "D:\\MoreWallpapers" }
        };

        try
        {
            AppSettingsStore.Save(settings, path);
            var restored = AppSettingsStore.Load(path);

            Assert.Equal(settings.ThemeMode, restored.ThemeMode);
            Assert.Equal(settings.Language, restored.Language);
            Assert.Equal(settings.Volume, restored.Volume);
            Assert.Equal(settings.LibraryPaths.Count, restored.LibraryPaths.Count);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
