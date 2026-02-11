using Gauniv.WpfClient.Models;

namespace Gauniv.WpfClient.Services;

/// <summary>
/// 游戏服务接口
/// </summary>
public interface IGameService
{
    /// <summary>
    /// 获取游戏列表
    /// </summary>
    Task<List<Game>> GetGamesAsync(int page = 1, int pageSize = 10, int? categoryId = null);
    
    /// <summary>
    /// 获取游戏详情
    /// </summary>
    Task<Game?> GetGameByIdAsync(int id);
    
    /// <summary>
    /// 获取类别列表
    /// </summary>
    Task<List<Category>> GetCategoriesAsync();
    
    /// <summary>
    /// 购买游戏
    /// </summary>
    Task<bool> PurchaseGameAsync(int gameId);
    
    /// <summary>
    /// 下载游戏
    /// </summary>
    Task<bool> DownloadGameAsync(int gameId, string downloadPath);
}
