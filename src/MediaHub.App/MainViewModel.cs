using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MediaHub.Core.Models;
using MediaHub.Data;
using MediaHub.Services;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace MediaHub.App;

public partial class MainViewModel : ObservableObject
{
    private readonly string _databasePath;
    private readonly FileScannerService _scanner;

    // Список, який показує таблиця у вікні
    public ObservableCollection<MediaFile> Files { get; } = new();

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private MediaFile? _selectedFile;
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

        using var db = new MediaDbContext(_databasePath);
        await db.Database.EnsureCreatedAsync();

        var query = db.MediaFiles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(text))
            query = query.Where(m => m.Title.Contains(text));

        var list = await query.OrderBy(m => m.Title).ToListAsync();

        Files.Clear();
        foreach (var file in list)
            Files.Add(file);
    }

    // Сканує вибрану папку й оновлює список
    public async Task ScanFolderAsync(string folder)
    {
        Status = "Сканування...";
        int added = await Task.Run(() => _scanner.ScanFolderAsync(folder));
        await LoadAsync();
        Status = $"Додано нових файлів: {added}";
    }

    // Викликається автоматично при зміні тексту в полі пошуку
    partial void OnSearchTextChanged(string value)
    {
        _ = LoadAsync();
    }
}