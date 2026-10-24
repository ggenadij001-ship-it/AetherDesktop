using AetherDesktop.Core.Configuration;
using AetherDesktop.Core.Models;
using AetherDesktop.Core.Services;
using Xunit;

namespace AetherDesktop.Core.Tests;

public class WallpaperLibraryServiceTests
{
    [Fact]
    public void AddFile_WithSupportedImage_AddsWallpaperToLibrary()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"aether-library-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var filePath = Path.Combine(tempDirectory, "sample.png");
            File.WriteAllText(filePath, "fake-png-content");

            var service = new WallpaperLibraryService(Path.Combine(tempDirectory, "library.json"));
            var item = service.AddFile(filePath);

            Assert.NotNull(item);
            Assert.Equal(WallpaperType.Image, item!.Type);
            Assert.Single(service.Items);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    [Fact]
    public void AddFile_WithUnsupportedExtension_ReturnsNull()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"aether-library-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var filePath = Path.Combine(tempDirectory, "notes.txt");
            File.WriteAllText(filePath, "hello");

            var service = new WallpaperLibraryService(Path.Combine(tempDirectory, "library.json"));
            var item = service.AddFile(filePath);

            Assert.Null(item);
            Assert.Empty(service.Items);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    [Fact]
    public void Search_FiltersByTitle()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"aether-library-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var filePath = Path.Combine(tempDirectory, "sunset.png");
            File.WriteAllText(filePath, "fake content");

            var service = new WallpaperLibraryService(Path.Combine(tempDirectory, "library.json"));
            service.AddFile(filePath);

            var results = service.Search("sunset");

            Assert.Single(results);
            Assert.Equal("sunset", results[0].Title, ignoreCase: true);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    [Fact]
    public void SaveAndLoad_RoundTripsLibrary()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"aether-library-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var filePath = Path.Combine(tempDirectory, "night.mp4");
            File.WriteAllText(filePath, "fake video data");

            var storagePath = Path.Combine(tempDirectory, "library.json");
            var service = new WallpaperLibraryService(storagePath);
            service.AddFile(filePath);

            var loadedService = new WallpaperLibraryService(storagePath);

            Assert.Single(loadedService.Items);
            Assert.Equal(WallpaperType.Video, loadedService.Items[0].Type);
            Assert.Equal(filePath, loadedService.Items[0].Path);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }
}
