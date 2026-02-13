using Microsoft.Extensions.DependencyInjection;

namespace Gauniv.WpfClient.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<object> _backStack = new();
    private object? _currentViewModel;

    public event Action? CurrentViewChanged;

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            CurrentViewChanged?.Invoke();
        }
    }

    public bool CanGoBack => _backStack.Count > 0;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        if (_currentViewModel != null)
        {
            _backStack.Push(_currentViewModel);
        }

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewModel = viewModel;
    }

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
    {
        if (_currentViewModel != null)
        {
            _backStack.Push(_currentViewModel);
        }

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        if (viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(parameter);
        }
        
        CurrentViewModel = viewModel;
    }

    public void NavigateToRoot<TViewModel>(object parameter) where TViewModel : class
    {
        _backStack.Clear();

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        if (viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(parameter);
        }
        
        CurrentViewModel = viewModel;
    }

    public void GoBack()
    {
        if (_backStack.Count > 0)
        {
            CurrentViewModel = _backStack.Pop();
        }
    }
}

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);
}
