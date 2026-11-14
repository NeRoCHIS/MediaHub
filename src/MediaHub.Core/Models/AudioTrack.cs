namespace MediaHub.Core.Models;

public class AudioTrack : MediaFile
{
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public int Bitrate { get; set; }
    public byte[]? CoverImage { get; set; }
    
    public override MediaType Type => MediaType.Audio;

    public override void ExtractMetadata()
    {
        using var file = TagLib.File.Create(FilePath);
        Title = ResolveTitle(file.Tag.Title);
        Album = file.Tag.Album;
        Genre = file.Tag.FirstGenre;
        Duration = file.Properties.Duration;
        Bitrate = file.Properties.AudioBitrate;
        FileSize = new FileInfo(FilePath).Length;
        CoverImage = file.Tag.Pictures.FirstOrDefault()?.Data.Data;
        
    }
    
    public override string GetDescription() =>
        $"{Artist ?? "Невідомий виконавець"} — {Title} ({Bitrate} kbps)";
}