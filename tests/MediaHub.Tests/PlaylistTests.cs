using MediaHub.Core.Models;
using MediaHub.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MediaHub.Tests;

public class PlaylistTests : IDisposable
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"mediahub_test_{Guid.NewGuid()}.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }

    [Fact]
    public async Task Playlist_KeepsItsFiles()
    {
        using (var db = new MediaDbContext(_dbPath))
        {
            await db.Database.EnsureCreatedAsync();

            var playlist = new Playlist { Name = "Gym" };
            playlist.Items.Add(new PlaylistItem
            {
                MediaFile = new AudioTrack { FilePath = "a.mp3", Title = "Song" }
            });

            db.Playlists.Add(playlist);
            await db.SaveChangesAsync();
        }

        using (var db = new MediaDbContext(_dbPath))
        {
            var playlist = await db.Playlists
                .Include(p => p.Items)
                .ThenInclude(i => i.MediaFile)
                .SingleAsync();

            Assert.Equal("Gym", playlist.Name);
            Assert.Single(playlist.Items);
            Assert.Equal("Song", playlist.Items[0].MediaFile.Title);
        }
    }
}