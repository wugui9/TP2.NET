using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;

namespace Gauniv.WebServer.Models
{
    // Home page view model
    public class StoreIndexViewModel
    {
        public List<GameListDto> LatestGames { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
    }

    // Game list view model
    public class StoreGamesViewModel
    {
        public List<GameListDto> Games { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int Limit { get; set; } = 12;
        public bool IsLoggedIn { get; set; }

        // Filter conditions (for display)
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? Owned { get; set; }
    }

    // Game detail view model
    public class StoreGameDetailViewModel
    {
        public GameDto Game { get; set; } = new();
        public bool IsLoggedIn { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
    }

    // Category list view model
    public class StoreCategoriesViewModel
    {
        public List<CategoryDto> Categories { get; set; } = new();
    }

    // Category detail view model
    public class StoreCategoryDetailViewModel
    {
        public CategoryDto Category { get; set; } = new();
        public List<GameListDto> Games { get; set; } = new();
    }

    // Library view model
    public class StoreLibraryViewModel
    {
        public List<GameListDto> OwnedGames { get; set; } = new();
        public string UserName { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public decimal TotalValue { get; set; }
    }

    // Profile view model
    public class StoreProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }

    // Login view model
    public class StoreLoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? Error { get; set; }
    }

    // Register view model
    public class StoreRegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    // Download view model
    public class StoreDownloadViewModel
    {
        public GameDto Game { get; set; } = new();
        public int GameId { get; set; }
    }
}
