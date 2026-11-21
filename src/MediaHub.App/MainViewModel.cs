using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MediaHub.Core.Models;
using MediaHub.Data;
using MediaHub.Services;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaHub.App;

public partial class MainViewModel : ObservableObject
{
    private readonly string _databasePath;
    private readonly FileScannerService _scanner;

    // Список, який показує таблиця у вікні
    public ObservableCollection<MediaFile> Files { get; } = new();
    public ObservableCollection<Playlist> Playlists { get; } = new();

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private MediaFile? _selectedFile;
    [ObservableProperty] private Playlist? _selectedPlaylist;
    [ObservableProperty] private string _newPlaylistName = string.Empty;
    [ObservableProperty] private string _status = "Готово";

    public MainViewModel()
    {
        // База лежить у папці користувача: ...\AppData\Local\MediaHub\media.db
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MediaHub");
        Directory.CreateDirectory(folder);

        _databasePath = Path.Combine(folder, "media.db");
        _scanner = new FileScannerService(_databasePath);
    }

    // Завантажує файли з бази (з урахуванням пошуку)
    public async Task LoadAsync()
    {
        var text = SearchText;
        var playlist = SelectedPlaylist;

        using var db = new MediaDbContext(_databasePath);
        await db.Database.EnsureCreatedAsync();

        IQueryable<MediaFile> query;
        if (playlist != null)
        {
            var playlistId = playlist.Id;
            query = db.PlaylistItems
                .Where(i => i.PlaylistId == playlistId)
                .Select(i => i.MediaFile);
        }
        else
        {
            query = db.MediaFiles;
        }

        if (!string.IsNullOrWhiteSpace(text))
            query = query.Where(m => m.Title.Contains(text));
        
        var list = await query.OrderBy(m => m.Title).ToListAsync();
        
        Files.Clear();
        foreach (var file in list)
            Files.Add(file);
    }

    // Завантажує список плейлистів
    public async Task LoadPlaylistsAsync()
    {
        using var db = new MediaDbContext(_databasePath);
        await db.Database.EnsureCreatedAsync();

        var list = await db.Playlists.OrderBy(p => p.Name).ToListAsync();

        Playlists.Clear();
        foreach (var playlist in list)
            Playlists.Add(playlist);
    }
    
    // Сканує вибрану папку й оновлює список
    public async Task ScanFolderAsync(string folder)
    {
        Status = "Сканування...";
        int added = await Task.Run(() => _scanner.ScanFolderAsync(folder));
        await LoadAsync();
        Status = $"Додано нових файлів: {added}";
    }
    
    // Створює новий плейлист
    public async Task CreatePlaylistAsync()
    {
        var name = NewPlaylistName.Trim();
        if (name == "")
        {
            Status = "Введіть назву плейлиста";
            return;
        }
        
        using var db = new MediaDbContext(_databasePath);
        var playlist = new Playlist { Name = name };
        db.Playlists.Add(playlist);
        await db.SaveChangesAsync();

        Playlists.Add(playlist);
        NewPlaylistName = string.Empty;
        Status = $"Створено плейлист: {name}";
    }

    // Додає вибраний у таблиці файл у плейлист
    public async Task AddToPlaylistAsync(Playlist playlist)
    {
        if (SelectedFile == null)
        {
            Status = "Спочатку виберіть файл у таблиці";
            return;
        }

        var fileId = SelectedFile.Id;

        using var db = new MediaDbContext(_databasePath);
        bool exists = await db.PlaylistItems
            .AnyAsync(i => i.PlaylistId == playlist.Id && i.MediaFileId == fileId);

        if (exists)
        {
            Status = "Цей файл уже є в плейлисті";
            return;
        }

        db.PlaylistItems.Add(new PlaylistItem { PlaylistId = playlist.Id, MediaFileId = fileId });
        await db.SaveChangesAsync();
        Status = $"Додано в плейлист «{playlist.Name}»";
    }
    
    // Прибирає вибраний файл із відкритого плейлиста
    public async Task RemoveFromPlaylistAsync()
    {
        if (SelectedPlaylist == null || SelectedFile == null)
        {
            Status = "Відкрийте плейлист і виберіть файл";
            return;
        }

        var playlistId = SelectedPlaylist.Id;
        var fileId = SelectedFile.Id;

        using var db = new MediaDbContext(_databasePath);
        var item = await db.PlaylistItems
            .FirstOrDefaultAsync(i => i.PlaylistId == playlistId && i.MediaFileId == fileId);

        if (item == null)
            return;

        db.PlaylistItems.Remove(item);
        await db.SaveChangesAsync();

        await LoadAsync();
        Status = "Файл прибрано з плейлиста";
    }
    
    // Видаляє вибраний плейлист (файли в бібліотеці лишаються)
    public async Task DeletePlaylistAsync()
    {
        if (SelectedPlaylist == null)
        {
            Status = "Виберіть плейлист";
            return;
        }

        var playlist = SelectedPlaylist;

        using var db = new MediaDbContext(_databasePath);
        var entity = await db.Playlists.FindAsync(playlist.Id);
        if (entity != null)
        {
            db.Playlists.Remove(entity);
            await db.SaveChangesAsync();
        }

        Playlists.Remove(playlist);
        Status = $"Плейлист «{playlist.Name}» видалено";
    }
    
    // Викликається автоматично при зміні тексту в полі пошуку
    partial void OnSearchTextChanged(string value) => _ = LoadAsync();
    
    partial void OnSelectedPlaylistChanged(Playlist? value) => _ = LoadAsync();
   
    
}