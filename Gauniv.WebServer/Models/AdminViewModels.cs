using Gauniv.WebServer.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Gauniv.WebServer.Models
{
    // Admin dashboard view model
    public class AdminDashboardViewModel
    {
        public int TotalGames { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<GameListDto> RecentGames { get; set; } = new();
        public List<AdminUserListItem> RecentUsers { get; set; } = new();
    }

    // Admin game list view model
    public class AdminGamesViewModel
    {
        public List<AdminGameListItem> Games { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchQuery { get; set; }
    }

    // Admin game list item
    public class AdminGameListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> CategoryNames { get; set; } = new();
        public int OwnerCount { get; set; }
    }

    // Create game view model
    public class AdminCreateGameViewModel
    {
        [Required(ErrorMessage = "Game name is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99")]
        public decimal Price { get; set; }

        public IFormFile? GameFile { get; set; }

        public List<int> SelectedCategoryIds { get; set; } = new();

        // Available categories (for checkboxes)
        public List<CategoryDto> AvailableCategories { get; set; } = new();

        public string? Error { get; set; }
    }

    // Edit game view model
    public class AdminEditGameViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Game name is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99")]
        public decimal Price { get; set; }

        public string? CurrentFileName { get; set; }
        public long CurrentSize { get; set; }

        public IFormFile? GameFile { get; set; }

        public List<int> SelectedCategoryIds { get; set; } = new();

        public List<CategoryDto> AvailableCategories { get; set; } = new();

        public string? Error { get; set; }
    }

    // Admin category list view model
    public class AdminCategoriesViewModel
    {
        public List<CategoryDto> Categories { get; set; } = new();
    }

    // Create category view model
    public class AdminCreateCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public string? Error { get; set; }
    }

    // Edit category view model
    public class AdminEditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public int GameCount { get; set; }

        public string? Error { get; set; }
    }

    // Admin user list view model
    public class AdminUsersViewModel
    {
        public List<AdminUserListItem> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchQuery { get; set; }
    }

    // Admin user list item
    public class AdminUserListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public int OwnedGameCount { get; set; }
        public bool IsAdmin { get; set; }
    }

    // Admin user detail view model
    public class AdminUserDetailViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public bool IsAdmin { get; set; }
        public List<GameListDto> OwnedGames { get; set; } = new();
    }
}
