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
    private string _currentSection = "Library";
    private WallpaperLibraryItem? _selectedItem;

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

        _selectedItem = _libraryService.Items.FirstOrDefault();
    }

    private void RefreshLibrary()
    {
        var items = GetVisibleItems();
        WallpaperItemsControl.ItemsSource = items.Select(item => new WallpaperListItemViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Path = item.Path,
            Type = item.Type,
            IsSelected = _selectedItem != null && item.Id == _selectedItem.Id
        }).ToList();

        if (_selectedItem is not null)
        {
            NowPlayingText.Text = _selectedItem.Title;
        }
        else if (items.Count > 0)
        {
            NowPlayingText.Text = items[0].Title;
        }
        else
        {
            NowPlayingText.Text = "No wallpaper loaded";
        }

        LibraryHeaderText.Text = _currentSection switch
        {
            "Library" => "Wallpaper library",
            "Installed" => "Installed wallpapers",
            "Favorites" => "Favorites",
            "Playlists" => "Playlists",
            "Displays" => "Displays",
            "Settings" => "Settings",
            "Import" => "Import",
            _ => "Wallpaper library"
        };

        LastUpdateText.Text = $"Last update: {DateTime.Now:HH:mm}";
    }

    private IReadOnlyList<WallpaperLibraryItem> GetVisibleItems()
    {
        var query = SearchTextBox.Text ?? string.Empty;
        var items = _libraryService.Search(query);

        return _currentSection switch
        {
            "Library" => items,
            "Installed" => items,
            "Favorites" => items.Where(x => x.IsFavorite).ToList(),
            "Playlists" => items.Where(x => x.Type == WallpaperType.Video || x.Type == WallpaperType.Image).ToList(),
            "Displays" => items,
            "Settings" => new List<WallpaperLibraryItem>(),
            "Import" => new List<WallpaperLibraryItem>(),
            _ => items
        };
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshLibrary();
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

    private void SidebarButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        _currentSection = button.Tag?.ToString() ?? "Library";
        RefreshLibrary();
    }

    private void WallpaperCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Border border)
        {
            return;
        }

        if (border.DataContext is not WallpaperListItemViewModel item)
        {
            return;
        }

        _selectedItem = _libraryService.Items.FirstOrDefault(x => x.Id == item.Id);
        RefreshLibrary();
    }

    private sealed class WallpaperListItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public WallpaperType Type { get; set; }
        public bool IsSelected { get; set; }
    }
}
