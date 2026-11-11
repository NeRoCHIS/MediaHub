namespace MediaHub.Core.Models;

public enum MediaType { Audio, Video }

public abstract class MediaFile
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Genre { get; set; }
    public TimeSpan Duration { get; set; }
    public long FileSize { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.Now;

    public abstract MediaType Type { get; }

    // Поліморфні методи: кожен нащадок реалізує по-своєму
    public abstract void ExtractMetadata();
    public abstract string GetDescription();

    protected string ResolveTitle(string? tagTitle) =>
        string.IsNullOrWhiteSpace(tagTitle)
            ? Path.GetFileNameWithoutExtension(FilePath)
            : tagTitle;
}