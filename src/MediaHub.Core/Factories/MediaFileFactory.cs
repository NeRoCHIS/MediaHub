using MediaHub.Core.Models;
using TagLib.Matroska;
using AudioTrack = MediaHub.Core.Models.AudioTrack;

namespace MediaHub.Core.Factories;

public static class MediaFileFactory
{
    private static readonly HashSet<string> AudioExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".mp3", ".flac", ".wav", ".ogg", ".m4a", ".aac" };

    private static readonly HashSet<string> VideoExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".mkv", ".avi", ".mov", ".wmv" };

    public static bool IsSupported(string path)
    {
        var ext = Path.GetExtension(path);
        return AudioExtensions.Contains(ext) || VideoExtensions.Contains(ext);
    }

    public static MediaFile? Create(string path)
    {
        var ext = Path.GetExtension(path);

        if (AudioExtensions.Contains(ext)) return new AudioTrack() { FilePath = path };
        if (VideoExtensions.Contains(ext)) return new VideoFile  { FilePath = path };
        return null;
    }
}