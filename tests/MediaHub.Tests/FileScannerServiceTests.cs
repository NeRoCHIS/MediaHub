//FileScannerService відповідає за наповнення бази.
//Він обходить папку, фабрика за розширенням створює потрібний клас,
//поліморфний метод ExtractMetadata() читає теги,
//а DbContext зберігає результат. Інтерфейс про це нічого не знає,
//тому логіку можна змінювати й тестувати окремо

using MediaHub.Data;
using MediaHub.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MediaHub.Tests;

public class FileScannerServiceTests : IDisposable
{
    private readonly string _folder =
        Path.Combine(Path.GetTempPath(), $"mediahub_scan_{Guid.NewGuid()}");

    private readonly string _dbPath;
    private readonly FileScannerService _scanner;

    public FileScannerServiceTests()
    {
        Directory.CreateDirectory(_folder);
        _dbPath = _folder + ".db";
        _scanner = new FileScannerService(_dbPath);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_folder, true);
        File.Delete(_dbPath);
    }

    [Fact]
    public async Task ScanFolder_AddsSupportedFilesAndIgnoresOthers()
    {
        CreateWavFile(Path.Combine(_folder, "sound.wav"));
        File.WriteAllText(Path.Combine(_folder,"notes.txt"), "not a media file");

        int added = await _scanner.ScanFolderAsync(_folder);
        
        Assert.Equal(1, added);
        using var db = new MediaDbContext(_dbPath);
        Assert.Equal(1,await db.MediaFiles.CountAsync());
    }

  

    [Fact]
    public async Task ScanFolder_Twice_DoesNotCreateDuplicates()
    {
        CreateWavFile(Path.Combine(_folder, "sound.wav"));

        await _scanner.ScanFolderAsync(_folder);
        int secondScan = await _scanner.ScanFolderAsync(_folder);
        
        Assert.Equal(0, secondScan);
    }

    [Fact]
    public async Task ScanFolder_CorruptFile_IsSkipped()
    {
        File.WriteAllText(Path.Combine(_folder, "broken.mp3"), "this is not real audio");

        int added = await _scanner.ScanFolderAsync(_folder);

        Assert.Equal(0, added);
    }

    private void CreateWavFile(string path)
    {
        const int dataSize = 8000;
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write("RIFF" .ToCharArray());
        writer.Write(36 + dataSize);
        writer.Write("WAVE".ToCharArray());
        writer.Write("fmt ".ToCharArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(8000);
        writer.Write(8000);
        writer.Write((short)1);
        writer.Write((short)8);
        writer.Write("data".ToCharArray());
        writer.Write(dataSize);
        writer.Write(new byte[dataSize]);
    }
    
}