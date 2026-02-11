using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.WpfClient.Models;
using Gauniv.WpfClient.Services;

namespace Gauniv.WpfClient.ViewModels;

/// <summary>
/// 游戏详情视图模型
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
    private bool _isPurchased;

    [ObservableProperty]
    private bool _isDownloaded;

    [ObservableProperty]
    private string _downloadPath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

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
            
            // TODO: 检查是否已购买和已下载
            // 这里需要额外的 API 或本地状态管理
            IsPurchased = false;
            IsDownloaded = false;
        }
        catch
        {
            // 处理错误
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
                StatusMessage = "游戏购买成功！";
            }
            else
            {
                StatusMessage = "购买失败，请重试。";
            }
        }
        catch
        {
            // 处理错误
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
            // 设置下载路径
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
                StatusMessage = "游戏下载成功！";
            }
            else
            {
                StatusMessage = "下载失败，请重试。";
            }
        }
        catch
        {
            // 处理错误
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
            // 使用默认程序打开文件
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = DownloadPath,
                UseShellExecute = true
            });
        }
        catch
        {
            // 处理错误
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<GameListViewModel>();
    }
}
