using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AetherDesktop.Core.Models;
using AetherDesktop.Core.Services;

namespace AetherDesktop.App;

public partial class MainWindow : Window
{
    private readonly WallpaperLibraryService _libraryService;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Aether Desktop";

        _libraryService = new WallpaperLibraryService();
        LoadSampleLibrary();
        RefreshLibrary();
    }

    private void LoadSampleLibrary()
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var sampleFolder = Path.Combine(appDataFolder, "AetherDesktop", "SampleLibrary");

        if (!Directory.Exists(sampleFolder))
        {
            Directory.CreateDirectory(sampleFolder);
        }

        var sampleFiles = new[]
        {
            Path.Combine(sampleFolder, "aurora.png"),
            Path.Combine(sampleFolder, "night.mp4"),
            Path.Combine(sampleFolder, "forest.jpg")
        };

        foreach (var samplePath in sampleFiles)
        {
            if (!File.Exists(samplePath))
            {
                File.WriteAllText(samplePath, "sample-data");
            }

            _libraryService.AddFile(samplePath);
        }
    }

    private void RefreshLibrary()
    {
        var items = _libraryService.Items;
        WallpaperItemsControl.ItemsSource = items;

        if (items.Count > 0)
        {
            NowPlayingText.Text = items[0].Title;
        }
        else
        {
            NowPlayingText.Text = "No wallpaper loaded";
        }

        LastUpdateText.Text = $"Last update: {DateTime.Now:HH:mm}";
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = SearchTextBox.Text ?? string.Empty;
        var filtered = _libraryService.Search(query);
        WallpaperItemsControl.ItemsSource = filtered;
    }

    private void OpenLibraryFolder_Click(object sender, RoutedEventArgs e)
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AetherDesktop");

        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = appDataFolder,
            UseShellExecute = true
        });
    }
}
