namespace Gauniv.WpfClient.Models;

/// <summary>
/// 登录请求模型
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
