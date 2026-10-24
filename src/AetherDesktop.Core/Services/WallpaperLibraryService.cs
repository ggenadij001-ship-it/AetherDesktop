using System.Text.Json;

namespace AetherDesktop.Core.Services;

public class WallpaperLibraryService
{
    private readonly string _storagePath;

    public WallpaperLibraryService(string? storagePath = null)
    {
        _storagePath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AetherDesktop",
            "library.json");

        var directory = Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Items = LoadItems();
    }

    public List<WallpaperLibraryItem> Items { get; private set; } = new();

    public WallpaperLibraryItem? AddFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A file path is required.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The wallpaper file does not exist.", filePath);
        }

        var item = CreateLibraryItem(filePath);
        if (item is null)
        {
            return null;
        }

        Items.Add(item);
        Save();
        return item;
    }

    public int AddFiles(IEnumerable<string> filePaths)
    {
        var count = 0;

        foreach (var filePath in filePaths)
        {
            if (AddFile(filePath) is not null)
            {
                count++;
            }
        }

        return count;
    }

    public void Remove(Guid itemId, bool deleteOriginalFile = false)
    {
        var item = Items.FirstOrDefault(x => x.Id == itemId);
        if (item is null)
        {
            return;
        }

        if (deleteOriginalFile && File.Exists(item.Path))
        {
            File.Delete(item.Path);
        }

        Items.Remove(item);
        Save();
    }

    public IReadOnlyList<WallpaperLibraryItem> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Items;
        }

        var normalized = query.Trim();
        return Items
            .Where(x => x.Title.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                || x.Path.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IReadOnlyList<WallpaperLibraryItem> Filter(WallpaperType? type = null, bool favoritesOnly = false)
    {
        var filtered = Items.AsEnumerable();

        if (type.HasValue)
        {
            filtered = filtered.Where(x => x.Type == type.Value);
        }

        if (favoritesOnly)
        {
            filtered = filtered.Where(x => x.IsFavorite);
        }

        return filtered.ToList();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Items, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_storagePath, json);
    }

    private List<WallpaperLibraryItem> LoadItems()
    {
        if (!File.Exists(_storagePath))
        {
            return new List<WallpaperLibraryItem>();
        }

        var json = File.ReadAllText(_storagePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<WallpaperLibraryItem>();
        }

        var items = JsonSerializer.Deserialize<List<WallpaperLibraryItem>>(json);
        return items ?? new List<WallpaperLibraryItem>();
    }

    private static WallpaperLibraryItem? CreateLibraryItem(string filePath)
    {
        var extension = Path.GetExtension(filePath).Trim().ToLowerInvariant();
        var type = DetectWallpaperType(extension);

        if (type is null)
        {
            return null;
        }

        var info = new FileInfo(filePath);
        return new WallpaperLibraryItem
        {
            Id = Guid.NewGuid(),
            Title = Path.GetFileNameWithoutExtension(filePath),
            Path = filePath,
            Type = type.Value,
            Description = string.Empty,
            AddedAt = DateTimeOffset.UtcNow,
            LastUsedAt = DateTimeOffset.UtcNow,
            ThumbnailPath = string.Empty,
            FileSizeBytes = info.Exists ? info.Length : 0,
            IsFavorite = false,
            IsValid = true,
            ErrorMessage = null
        };
    }

    private static WallpaperType? DetectWallpaperType(string extension)
    {
        return extension switch
        {
            ".png" or ".jpg" or ".jpeg" or ".webp" => WallpaperType.Image,
            ".mp4" or ".mov" or ".mkv" or ".avi" => WallpaperType.Video,
            ".html" or ".htm" => WallpaperType.Web,
            _ => null
        };
    }
}
