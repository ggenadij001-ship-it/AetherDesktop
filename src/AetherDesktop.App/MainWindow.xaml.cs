using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        UpdatePreviewDetails();

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

    private void UpdatePreviewDetails()
    {
        if (_selectedItem is null || !File.Exists(_selectedItem.Path))
        {
            PreviewTitle.Text = "No wallpaper selected";
            PreviewType.Text = "Type: —";
            PreviewPath.Text = "Path: —";
            PreviewStatus.Text = "Status: Select a wallpaper";
            PreviewPlaceholderText.Text = "Preview";
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
            PreviewPlaceholderText.Visibility = Visibility.Visible;
            UpdateBackgroundPreview();
            return;
        }

        PreviewTitle.Text = _selectedItem.Title;
        PreviewType.Text = $"Type: {_selectedItem.Type}";
        PreviewPath.Text = $"Path: {_selectedItem.Path}";
        PreviewStatus.Text = _selectedItem.IsFavorite ? "Status: Active and in favorites" : "Status: Ready to apply";

        if (_selectedItem.Type == WallpaperType.Image)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_selectedItem.Path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 640;
                bitmap.EndInit();
                bitmap.Freeze();

                PreviewImage.Source = bitmap;
                PreviewImage.Visibility = Visibility.Visible;
                PreviewPlaceholderText.Visibility = Visibility.Collapsed;
            }
            catch
            {
                PreviewImage.Source = null;
                PreviewImage.Visibility = Visibility.Collapsed;
                PreviewPlaceholderText.Visibility = Visibility.Visible;
                PreviewPlaceholderText.Text = "Preview unavailable";
            }
        }
        else
        {
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
            PreviewPlaceholderText.Visibility = Visibility.Visible;
            PreviewPlaceholderText.Text = _selectedItem.Type.ToString();
        }

        UpdateBackgroundPreview();
    }

    private void UpdateBackgroundPreview()
    {
        if (_selectedItem is null || !File.Exists(_selectedItem.Path) || _selectedItem.Type != WallpaperType.Image)
        {
            WallpaperBackground.Background = new SolidColorBrush(Color.FromRgb(18, 24, 38));
            return;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(_selectedItem.Path, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            WallpaperBackground.Background = new ImageBrush(bitmap)
            {
                Stretch = Stretch.UniformToFill,
                Opacity = 0.38
            };
        }
        catch
        {
            WallpaperBackground.Background = new SolidColorBrush(Color.FromRgb(18, 24, 38));
        }
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

        var selected = _libraryService.Items.FirstOrDefault(x => x.Id == item.Id);
        if (selected is null)
        {
            return;
        }

        _selectedItem = selected;
        RefreshLibrary();
    }

    private void ApplyWallpaper_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedItem is null)
        {
            PreviewStatus.Text = "Status: No wallpaper selected";
            return;
        }

        _selectedItem.IsFavorite = true;
        PreviewStatus.Text = $"Status: Applied '{_selectedItem.Title}'";
        NowPlayingText.Text = _selectedItem.Title;
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
