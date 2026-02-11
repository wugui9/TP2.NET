namespace Gauniv.WpfClient.Services;

/// <summary>
/// 导航服务接口
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// 当前视图发生变化时触发
    /// </summary>
    event Action? CurrentViewChanged;
    
    /// <summary>
    /// 当前视图模型
    /// </summary>
    object? CurrentViewModel { get; }
    
    /// <summary>
    /// 导航到指定视图模型
    /// </summary>
    void NavigateTo<TViewModel>() where TViewModel : class;
    
    /// <summary>
    /// 导航到指定视图模型（带参数）
    /// </summary>
    void NavigateTo<TViewModel>(object parameter) where TViewModel : class;
}
