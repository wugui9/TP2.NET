using Gauniv.WpfClient.Models;

namespace Gauniv.WpfClient.Services;

public interface IGameService
{
    Task<GameListResponse> GetGamesAsync(int offset = 0, int limit = 10, int[]? categoryIds = null, bool? owned = null, decimal? minPrice = null, decimal? maxPrice = null);
    
    Task<Game?> GetGameByIdAsync(int id);
    
    Task<List<Category>> GetCategoriesAsync();
    
    Task<bool> PurchaseGameAsync(int gameId);
    
    Task<bool> DownloadGameAsync(int gameId, string downloadPath);
}
