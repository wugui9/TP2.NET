using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.WpfClient.Models;
using Gauniv.WpfClient.Services;

namespace Gauniv.WpfClient.ViewModels;

/// <summary>
/// Game List ViewModel
/// </summary>
public partial class GameListViewModel : ViewModelBase, INavigationAware
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
    private int _totalCount;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private bool _hasNextPage = true;

    /// <summary>
    /// Whether to show only owned games
    /// </summary>
    [ObservableProperty]
    private bool _showOwnedOnly;

    /// <summary>
    /// Min price filter (null means no limit)
    /// </summary>
    [ObservableProperty]
    private string _minPriceText = string.Empty;

    /// <summary>
    /// Max price filter (null means no limit)
    /// </summary>
    [ObservableProperty]
    private string _maxPriceText = string.Empty;

    /// <summary>
    /// Page title (changes dynamically based on filter mode)
    /// </summary>
    public string PageTitle => ShowOwnedOnly ? "My Library" : "Game Store";

    public GameListViewModel(IGameService gameService, INavigationService navigationService)
    {
        _gameService = gameService;
        _navigationService = navigationService;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is bool showOwned)
        {
            ShowOwnedOnly = showOwned;
        }
        OnPropertyChanged(nameof(PageTitle));
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
            
            // Add "All" option
            Categories.Insert(0, new Category { Id = 0, Name = "All" });
            SelectedCategory = Categories[0];
        }
        catch
        {
            // Handle error
        }
    }

    private async Task LoadGamesAsync()
    {
        IsLoading = true;
        
        try
        {
            // Build filter parameters
            int[]? categoryIds = null;
            if (SelectedCategory is { Id: > 0 })
            {
                categoryIds = new[] { SelectedCategory.Id };
            }

            bool? owned = ShowOwnedOnly ? true : null;
            int offset = (CurrentPage - 1) * PageSize;

            decimal? minPrice = decimal.TryParse(MinPriceText, out var min) ? min : null;
            decimal? maxPrice = decimal.TryParse(MaxPriceText, out var max) ? max : null;

            var result = await _gameService.GetGamesAsync(offset, PageSize, categoryIds, owned, minPrice, maxPrice);
            
            Games = new ObservableCollection<Game>(result.Games);
            TotalCount = result.Total;
            
            // Update pagination button state
            HasPreviousPage = CurrentPage > 1;
            HasNextPage = offset + PageSize < result.Total;
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
    private async Task ApplyPriceFilterAsync()
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
