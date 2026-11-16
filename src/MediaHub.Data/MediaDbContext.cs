using MediaHub.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaHub.Data;

public class MediaDbContext : DbContext
{
    private readonly string _databasePath;

    public MediaDbContext(string databasePath)
    {
        _databasePath = databasePath;
    }

    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<AudioTrack> AudioTracks => Set<AudioTrack>();
    public DbSet<VideoFile> VideoFile => Set<VideoFile>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_databasePath}");
    }
    
}

