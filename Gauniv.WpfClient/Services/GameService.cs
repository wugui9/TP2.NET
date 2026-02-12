using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Gauniv.WpfClient.Models;

namespace Gauniv.WpfClient.Services;

public class GameService : IGameService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GameService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GameListResponse> GetGamesAsync(int offset = 0, int limit = 10, int[]? categoryIds = null, bool? owned = null, decimal? minPrice = null, decimal? maxPrice = null)
    {
        var empty = new GameListResponse();
        try
        {
            var url = $"/api/games?offset={offset}&limit={limit}";

            if (categoryIds is { Length: > 0 })
            {
                foreach (var id in categoryIds)
                {
                    url += $"&category={id}";
                }
            }

            if (owned.HasValue)
            {
                url += $"&owned={owned.Value.ToString().ToLower()}";
            }

            if (minPrice.HasValue)
            {
                url += $"&minPrice={minPrice.Value}";
            }

            if (maxPrice.HasValue)
            {
                url += $"&maxPrice={maxPrice.Value}";
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GameListResponse>(JsonOptions);
                return result ?? empty;
            }

            return empty;
        }
        catch
        {
            return empty;
        }
    }

    public async Task<Game?> GetGameByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/games/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Game>(JsonOptions);
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
                var categories = await response.Content.ReadFromJsonAsync<List<Category>>(JsonOptions);
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
