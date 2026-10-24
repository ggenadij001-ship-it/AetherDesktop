using AetherDesktop.Core.Configuration;
using AetherDesktop.Core.Models;

namespace AetherDesktop.Core.Services;

public interface IWallpaperSource
{
    WallpaperType Type { get; }
    string Name { get; }
    bool IsReady { get; }
    void Prepare();
    void Start();
    void Pause();
    void Resume();
    void Stop();
    void Dispose();
}

public abstract class WallpaperSourceBase : IWallpaperSource
{
    protected WallpaperSourceBase(string name, WallpaperType type)
    {
        Name = name;
        Type = type;
    }

    public WallpaperType Type { get; }
    public string Name { get; }
    public bool IsReady { get; protected set; }
    public PlaybackState State { get; protected set; } = PlaybackState.Stopped;

    public virtual void Prepare()
    {
        IsReady = true;
        State = PlaybackState.Stopped;
        AppLogger.Log(nameof(WallpaperSourceBase), $"Prepared source '{Name}'.");
    }

    public virtual void Start()
    {
        IsReady = true;
        State = PlaybackState.Playing;
        AppLogger.Log(nameof(WallpaperSourceBase), $"Started source '{Name}'.");
    }

    public virtual void Pause()
    {
        if (State == PlaybackState.Playing)
        {
            State = PlaybackState.Paused;
        }
    }

    public virtual void Resume()
    {
        if (State == PlaybackState.Paused)
        {
            State = PlaybackState.Playing;
        }
    }

    public virtual void Stop()
    {
        State = PlaybackState.Stopped;
        AppLogger.Log(nameof(WallpaperSourceBase), $"Stopped source '{Name}'.");
    }

    public virtual void Dispose()
    {
        State = PlaybackState.Stopped;
        IsReady = false;
    }
}

public class WallpaperSession
{
    public string SessionName { get; set; } = "Default";
    public List<WallpaperLibraryItem> Wallpapers { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
}
