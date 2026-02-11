using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using Gauniv.WpfClient.Models;

namespace Gauniv.WpfClient.Services;

/// <summary>
/// 游戏服务实现
/// </summary>
public class GameService : IGameService
{
    private readonly HttpClient _httpClient;

    public GameService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Game>> GetGamesAsync(int page = 1, int pageSize = 10, int? categoryId = null)
    {
        try
        {
            var url = $"/api/games?page={page}&pageSize={pageSize}";
            if (categoryId.HasValue)
            {
                url += $"&categoryId={categoryId.Value}";
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var games = await response.Content.ReadFromJsonAsync<List<Game>>();
                return games ?? new List<Game>();
            }
            
            return new List<Game>();
        }
        catch
        {
            return new List<Game>();
        }
    }

    public async Task<Game?> GetGameByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/games/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Game>();
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/categories");
            if (response.IsSuccessStatusCode)
            {
                var categories = await response.Content.ReadFromJsonAsync<List<Category>>();
                return categories ?? new List<Category>();
            }
            
            return new List<Category>();
        }
        catch
        {
            return new List<Category>();
        }
    }

    public async Task<bool> PurchaseGameAsync(int gameId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/games/{gameId}/purchase", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DownloadGameAsync(int gameId, string downloadPath)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/games/{gameId}/download");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(downloadPath, content);
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}
