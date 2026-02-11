using CommunityToolkit.Mvvm.ComponentModel;
using Gauniv.WpfClient.Services;

namespace Gauniv.WpfClient.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private object? _currentViewModel;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        // 订阅导航变化
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        
        // 默认导航到登录页面
        _navigationService.NavigateTo<LoginViewModel>();
    }

    private void OnCurrentViewChanged()
    {
        CurrentViewModel = _navigationService.CurrentViewModel;
    }
}
