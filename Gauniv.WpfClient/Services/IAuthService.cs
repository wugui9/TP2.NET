using Gauniv.WpfClient.Models;

namespace Gauniv.WpfClient.Services;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 当前登录的用户
    /// </summary>
    User? CurrentUser { get; }
    
    /// <summary>
    /// 访问令牌
    /// </summary>
    string? Token { get; }
    
    /// <summary>
    /// 是否已登录
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// 登录
    /// </summary>
    Task<bool> LoginAsync(string username, string password);
    
    /// <summary>
    /// 登出
    /// </summary>
    void Logout();
}
