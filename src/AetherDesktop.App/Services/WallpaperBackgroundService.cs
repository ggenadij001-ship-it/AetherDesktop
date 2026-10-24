using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AetherDesktop.App.Services;

public static class WallpaperBackgroundService
{
    public static ImageSource? LoadPreviewImage(string? imagePath, int decodePixelWidth = 640)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return null;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = decodePixelWidth;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public static Brush CreateBackgroundBrush(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return new SolidColorBrush(Color.FromRgb(18, 24, 38));
        }

        var image = LoadPreviewImage(imagePath, 1280);
        if (image is null)
        {
            return new SolidColorBrush(Color.FromRgb(18, 24, 38));
        }

        return new ImageBrush((ImageSource)image)
        {
            Stretch = Stretch.UniformToFill,
            Opacity = 0.38
        };
    }
}
