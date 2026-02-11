namespace Gauniv.WpfClient.Models;

/// <summary>
/// 登录响应模型
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public User User { get; set; } = new();
}
