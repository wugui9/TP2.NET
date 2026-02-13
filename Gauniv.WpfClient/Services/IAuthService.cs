using Gauniv.WpfClient.Models;

namespace Gauniv.WpfClient.Services;

public interface IAuthService
{
    User? CurrentUser { get; }
    
    bool IsAuthenticated { get; }
    
    Task<bool> LoginAsync(string email, string password);
    
    Task<User?> GetCurrentUserAsync();
    
    void Logout();
}
