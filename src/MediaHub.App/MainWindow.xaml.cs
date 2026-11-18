using System.Windows;
using Microsoft.Win32;

namespace MediaHub.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
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
}