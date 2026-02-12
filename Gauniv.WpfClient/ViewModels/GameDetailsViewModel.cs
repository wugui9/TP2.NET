using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.WpfClient.Models;
using Gauniv.WpfClient.Services;

namespace Gauniv.WpfClient.ViewModels;

/// <summary>
/// Game Details ViewModel
/// </summary>
public partial class GameDetailsViewModel : ViewModelBase, INavigationAware
{
    private readonly IGameService _gameService;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Game? _game;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDownloadButton))]
    private bool _isPurchased;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDownloadButton))]
    private bool _isDownloaded;

    [ObservableProperty]
    private string _downloadPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Show download button when purchased but not yet downloaded
    /// </summary>
    public bool ShowDownloadButton => IsPurchased && !IsDownloaded;


    public GameDetailsViewModel(
        IGameService gameService, 
        IAuthService authService,
        INavigationService navigationService)
    {
        _gameService = gameService;
        _authService = authService;
        _navigationService = navigationService;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is int gameId)
        {
            _ = LoadGameAsync(gameId);
        }
    }

    private async Task LoadGameAsync(int gameId)
    {
        IsLoading = true;
        
        try
        {
            Game = await _gameService.GetGameByIdAsync(gameId);
            
            if (Game != null)
            {
                // Use IsOwned field from API response to check ownership
                IsPurchased = Game.IsOwned;

                // Check if locally downloaded
                var downloadsFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Gauniv", "Games");
                var filePath = Path.Combine(downloadsFolder, $"{Game.Name}.txt");
                if (File.Exists(filePath))
                {
                    IsDownloaded = true;
                    DownloadPath = filePath;
                }
                else
                {
                    IsDownloaded = false;
                    DownloadPath = string.Empty;
                }
            }
        }
        catch
        {
            // Handle error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PurchaseAsync()
    {
        if (Game == null) return;

        IsLoading = true;
        
        try
        {
            var success = await _gameService.PurchaseGameAsync(Game.Id);
            if (success)
            {
                IsPurchased = true;
                StatusMessage = "Purchase successful!";
            }
            else
            {
                StatusMessage = "Purchase failed. Please try again.";
            }
        }
        catch
        {
            // Handle error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (Game == null || !IsPurchased) return;

        IsLoading = true;
        
        try
        {
            // Set download path
            var downloadsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Gauniv", "Games");
            
            Directory.CreateDirectory(downloadsFolder);
            
            var filePath = Path.Combine(downloadsFolder, $"{Game.Name}.txt");
            
            var success = await _gameService.DownloadGameAsync(Game.Id, filePath);
            if (success)
            {
                IsDownloaded = true;
                DownloadPath = filePath;
                StatusMessage = "Download successful!";
            }
            else
            {
                StatusMessage = "Download failed. Please try again.";
            }
        }
        catch
        {
            // Handle error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Launch()
    {
        if (!IsDownloaded || string.IsNullOrEmpty(DownloadPath)) return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = DownloadPath,
                UseShellExecute = true
            });
        }
        catch
        {
            // Handle error
        }
    }

    [RelayCommand]
    private void DeleteGame()
    {
        if (!IsDownloaded || string.IsNullOrEmpty(DownloadPath)) return;

        try
        {
            if (File.Exists(DownloadPath))
            {
                File.Delete(DownloadPath);
            }

            IsDownloaded = false;
            DownloadPath = string.Empty;
            StatusMessage = "Game deleted successfully.";
        }
        catch
        {
            StatusMessage = "Failed to delete the game.";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
        else
        {
            _navigationService.NavigateTo<GameListViewModel>();
        }
    }
}
