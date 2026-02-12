using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.WpfClient.Services;

namespace Gauniv.WpfClient.ViewModels;

/// <summary>
/// Main ViewModel
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private object? _currentViewModel;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private bool _isStoreActive = true;

    [ObservableProperty]
    private bool _isLibraryActive;

    [ObservableProperty]
    private bool _isProfileActive;

    public MainViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
        
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        _navigationService.NavigateTo<LoginViewModel>();
    }

    private void OnCurrentViewChanged()
    {
        CurrentViewModel = _navigationService.CurrentViewModel;
        IsLoggedIn = _authService.IsAuthenticated;
    }

    private void ResetTabs()
    {
        IsStoreActive = false;
        IsLibraryActive = false;
        IsProfileActive = false;
    }

    [RelayCommand]
    private void GoToStore()
    {
        ResetTabs();
        IsStoreActive = true;
        _navigationService.NavigateToRoot<GameListViewModel>(false);
    }

    [RelayCommand]
    private void GoToLibrary()
    {
        ResetTabs();
        IsLibraryActive = true;
        _navigationService.NavigateToRoot<GameListViewModel>(true);
    }

    [RelayCommand]
    private void GoToProfile()
    {
        ResetTabs();
        IsProfileActive = true;
        _navigationService.NavigateToRoot<ProfileViewModel>(true);
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        IsLoggedIn = false;
        ResetTabs();
        IsStoreActive = true;
        _navigationService.NavigateToRoot<LoginViewModel>(false);
    }
}
