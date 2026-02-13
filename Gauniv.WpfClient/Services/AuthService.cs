using System.Net.Http;
using System.Net.Http.Json;
using Gauniv.WpfClient.Models;

namespace Gauniv.WpfClient.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null && loginResponse.Success)
                {
                    CurrentUser = new User
                    {
                        Email = loginResponse.Email,
                        FirstName = loginResponse.FirstName,
                        LastName = loginResponse.LastName
                    };
                    
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

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/auth/me");
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<User>();
                if (user != null)
                {
                    CurrentUser = user;
                }
                return user;
            }
            return CurrentUser;
        }
        catch
        {
            return CurrentUser;
        }
    }

    public void Logout()
    {
        CurrentUser = null;
        _ = _httpClient.PostAsync("/api/auth/logout", null);
    }
}
