namespace MediaHub.Core.Models;

public class VideoFile : MediaFile
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string? SubtitlePath { get; set; }

    public override MediaType Type => MediaType.Video;

    public override void ExtractMetadata()
    {
        using var file = TagLib.File.Create(FilePath);
        Title = ResolveTitle(file.Tag.Title);
        Genre = file.Tag.FirstGenre;
        Duration = file.Properties.Duration;
        Width = file.Properties.VideoWidth;
        Height = file.Properties.VideoHeight;
        FileSize = new FileInfo(FilePath).Length;

        var srt = Path.ChangeExtension(FilePath, ".srt");
        SubtitlePath = System.IO.File.Exists(srt) ? srt : null;
    }

    public override string GetDescription() =>
        $"{Title} ({Width} x {Height})" + (SubtitlePath != null ? ", Є субтитри" : "");
    
}