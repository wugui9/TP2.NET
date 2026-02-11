using System.Net.Http;
using System.Net.Http.Json;
using Gauniv.WpfClient.Models;

namespace Gauniv.WpfClient.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    
    public User? CurrentUser { get; private set; }
    public string? Token { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && CurrentUser != null;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null)
                {
                    Token = loginResponse.Token;
                    CurrentUser = loginResponse.User;
                    
                    // 设置 HTTP 客户端的默认授权头
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                    
                    return true;
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public void Logout()
    {
        Token = null;
        CurrentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
