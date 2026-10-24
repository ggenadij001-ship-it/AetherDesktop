namespace AetherDesktop.Core.Models;

public class WallpaperLibraryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public WallpaperType Type { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUsedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ThumbnailPath { get; set; }
    public long FileSizeBytes { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsValid { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
