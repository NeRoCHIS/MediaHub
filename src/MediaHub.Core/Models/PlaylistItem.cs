namespace MediaHub.Core.Models;

public class PlaylistItem
{
    public int Id { get; set; }
    public int PlaylistId { get; set; }
    public int MediaFileId { get; set; }
    public MediaFile MediaFile { get; set; } = null!;
}