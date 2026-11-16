using MediaHub.Core.Models;
using MediaHub.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MediaHub.Tests;

public class MediaDbContextTests : IDisposable
{
    private readonly string _dbPath = 
        Path.Combine(Path.GetTempPath(), $"mediahub_test_{Guid.NewGuid()}.db");
    
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }

    [Fact]
    public async Task SavesAndReadsBothMediaTypes()
    {
        using (var db = new MediaDbContext(_dbPath))
        {
            await db.Database.EnsureCreatedAsync();
            db.MediaFiles.Add(new AudioTrack { FilePath = "a.mp3", Title = "Song", Artist = "Band" });
            db.MediaFiles.Add(new VideoFile { FilePath = "b.mp4", Title = "Film", Width = 1920 });
            await db.SaveChangesAsync();
        }

        using (var db = new MediaDbContext(_dbPath))
        {
            var all = await db.MediaFiles.ToListAsync();

            Assert.Equal(2, all.Count);
            Assert.Contains(all, f => f is AudioTrack a && a.Artist == "Band");
            Assert.Contains(all, f => f is VideoFile v && v.Width == 1920);
        }
    }
    
}