using System.Windows;
using System.Windows.Input;
using MediaHub.Core.Models;
using Microsoft.Win32;

namespace MediaHub.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private readonly PlayerService _player;
    private MediaFile? _currentFile;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        _player = new PlayerService(Player);
        _player.Volume = VolumeSlider.Value;
        _player.PositionChanged += UpdateProgress;
        VolumeSlider.ValueChanged += (_, e) => _player.Volume = e.NewValue;

        Loaded += async (_, _) => await _viewModel.LoadAsync();
    }

    // Кнопка "Додати папку": діалог вибору папки, потім сканування
    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.ScanFolderAsync(dialog.FolderName);
        }
    }

    // Подвійний клік по рядку: відтворити вибраний файл
    private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.SelectedFile != null)
            StartPlayback(_viewModel.SelectedFile);
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = _viewModel.SelectedFile;

        if (selected != null && selected != _currentFile)
            StartPlayback(selected);   // вибрано інший файл: граємо його
        else if (_currentFile != null)
            _player.Resume();          // той самий файл: продовжуємо після паузи
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e) => _player.Pause();

    private void StopButton_Click(object sender, RoutedEventArgs e) => _player.Stop();

    private void StartPlayback(MediaFile file)
    {
        _currentFile = file;
        _player.Play(file);
        _viewModel.Status = $"Грає: {file.Title}";
    }

    // Коли файл відкрито, знаємо його тривалість: задаємо межу смуги прогресу
    private void Player_MediaOpened(object sender, RoutedEventArgs e)
    {
        PlayProgress.Maximum = _player.Duration.TotalSeconds;
    }

    private void Player_MediaEnded(object sender, RoutedEventArgs e) => _player.Stop();

    private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        _viewModel.Status = "Не вдалося відтворити файл: " + e.ErrorException.Message;
    }

    // Оновлює смугу прогресу й текст часу
    private void UpdateProgress()
    {
        PlayProgress.Value = _player.Position.TotalSeconds;
        TimeText.Text = $"{_player.Position:h\\:mm\\:ss} / {_player.Duration:h\\:mm\\:ss}";
    }
}