namespace MediaHub.Core.Models;

public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PlaylistItem> Items { get; set; } = new();
}