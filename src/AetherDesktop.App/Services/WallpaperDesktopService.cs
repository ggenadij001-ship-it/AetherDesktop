using System;
using System.Runtime.InteropServices;

namespace AetherDesktop.App.Services;

public static class WallpaperDesktopService
{
    private const int SPI_SETDESKWALLPAPER = 0x0014;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SystemParametersInfo(
        int uAction,
        int uParam,
        string lpvParam,
        int fuWinIni);

    public static bool SetWallpaper(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return false;
        }

        try
        {
            var result = SystemParametersInfo(
                SPI_SETDESKWALLPAPER,
                0,
                imagePath,
                SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

            return result != 0;
        }
        catch
        {
            return false;
        }
    }
}
