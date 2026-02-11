using Microsoft.Extensions.DependencyInjection;

namespace Gauniv.WpfClient.Services;

/// <summary>
/// 导航服务实现
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
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

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewModel = viewModel;
    }

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        // 如果 ViewModel 实现了参数接口，则传递参数
        if (viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(parameter);
        }
        
        CurrentViewModel = viewModel;
    }
}

/// <summary>
/// 导航感知接口，用于接收导航参数
/// </summary>
public interface INavigationAware
{
    void OnNavigatedTo(object parameter);
}
