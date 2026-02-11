namespace Gauniv.WpfClient.Models;

/// <summary>
/// 用户模型
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Wallet { get; set; }
}
