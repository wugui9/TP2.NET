using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;

namespace Gauniv.WebServer.Models
{
    // 首页视图模型
    public class StoreIndexViewModel
    {
        public List<GameListDto> LatestGames { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
    }

    // 游戏列表视图模型
    public class StoreGamesViewModel
    {
        public List<GameListDto> Games { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int Limit { get; set; } = 12;
        public bool IsLoggedIn { get; set; }
    }

    // 游戏详情视图模型
    public class StoreGameDetailViewModel
    {
        public GameDto Game { get; set; } = new();
        public bool IsLoggedIn { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
    }

    // 分类列表视图模型
    public class StoreCategoriesViewModel
    {
        public List<CategoryDto> Categories { get; set; } = new();
    }

    // 分类详情视图模型
    public class StoreCategoryDetailViewModel
    {
        public CategoryDto Category { get; set; } = new();
        public List<GameListDto> Games { get; set; } = new();
    }

    // 游戏库视图模型
    public class StoreLibraryViewModel
    {
        public List<GameListDto> OwnedGames { get; set; } = new();
        public string UserName { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public decimal TotalValue { get; set; }
    }

    // 个人信息视图模型
    public class StoreProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }

    // 登录视图模型
    public class StoreLoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? Error { get; set; }
    }

    // 注册视图模型
    public class StoreRegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    // 下载视图模型
    public class StoreDownloadViewModel
    {
        public GameDto Game { get; set; } = new();
        public int GameId { get; set; }
    }
}
