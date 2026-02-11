using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.WpfClient.Models;
using Gauniv.WpfClient.Services;

namespace Gauniv.WpfClient.ViewModels;

/// <summary>
/// 游戏列表视图模型
/// </summary>
public partial class GameListViewModel : ViewModelBase
{
    private readonly IGameService _gameService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Game> _games = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private bool _hasNextPage = true;

    public GameListViewModel(IGameService gameService, INavigationService navigationService)
    {
        _gameService = gameService;
        _navigationService = navigationService;
        
        // 初始化时加载数据
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadCategoriesAsync();
        await LoadGamesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _gameService.GetCategoriesAsync();
            Categories = new ObservableCollection<Category>(categories);
            
            // 添加"全部"选项
            Categories.Insert(0, new Category { Id = 0, Name = "全部" });
            SelectedCategory = Categories[0];
        }
        catch
        {
            // 处理错误
        }
    }

    private async Task LoadGamesAsync()
    {
        IsLoading = true;
        
        try
        {
            var categoryId = SelectedCategory?.Id > 0 ? SelectedCategory.Id : (int?)null;
            var games = await _gameService.GetGamesAsync(CurrentPage, PageSize, categoryId);
            
            Games = new ObservableCollection<Game>(games);
            
            // 更新分页按钮状态
            HasPreviousPage = CurrentPage > 1;
            HasNextPage = games.Count >= PageSize;
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
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadGamesAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await LoadGamesAsync();
    }

    [RelayCommand]
    private async Task FilterByCategoryAsync()
    {
        CurrentPage = 1;
        await LoadGamesAsync();
    }

    [RelayCommand]
    private void ShowGameDetails(Game game)
    {
        if (game != null)
        {
            _navigationService.NavigateTo<GameDetailsViewModel>(game.Id);
        }
    }
}
