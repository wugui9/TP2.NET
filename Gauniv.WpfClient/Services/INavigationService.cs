namespace Gauniv.WpfClient.Services;

public interface INavigationService
{
    event Action? CurrentViewChanged;
    
    object? CurrentViewModel { get; }
    
    bool CanGoBack { get; }
    
    void NavigateTo<TViewModel>() where TViewModel : class;
    
    void NavigateTo<TViewModel>(object parameter) where TViewModel : class;
    
    /// <summary>
    /// Top-level navigation: clears back stack
    /// </summary>
    void NavigateToRoot<TViewModel>(object parameter) where TViewModel : class;
    
    void GoBack();
}
