using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Gauniv.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gauniv.WebServer.Controllers
{
    /// <summary>
    /// Admin controller - Game/Category/User management
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext dbContext,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #region Dashboard

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var local_totalGames = await _dbContext.Games.CountAsync();
            var local_totalCategories = await _dbContext.Categories.CountAsync();
            var local_totalUsers = await _dbContext.Users.CountAsync();

            var local_recentGames = await _dbContext.Games
                .Include(g => g.Categories)
                .Include(g => g.Owners)
                .OrderByDescending(g => g.CreatedAt)
                .Take(5)
                .Select(g => new GameListDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Price = g.Price,
                    Size = g.Size,
                    CreatedAt = g.CreatedAt,
                    CategoryNames = g.Categories.Select(c => c.Name).ToList()
                })
                .ToListAsync();

            var local_recentUsers = await _dbContext.Users
                .Include(u => u.OwnedGames)
                .OrderByDescending(u => u.RegisteredAt)
                .Take(5)
                .Select(u => new AdminUserListItem
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    RegisteredAt = u.RegisteredAt,
                    OwnedGameCount = u.OwnedGames.Count
                })
                .ToListAsync();

            var local_viewModel = new AdminDashboardViewModel
            {
                TotalGames = local_totalGames,
                TotalCategories = local_totalCategories,
                TotalUsers = local_totalUsers,
                TotalRevenue = local_recentGames.Sum(g => g.Price),
                RecentGames = local_recentGames,
                RecentUsers = local_recentUsers
            };

            return View(local_viewModel);
        }

        #endregion

        #region Game Management

        // GET: /Admin/Games
        public async Task<IActionResult> Games(int page = 1, string? search = null)
        {
            var local_limit = 15;
            var local_offset = (page - 1) * local_limit;

            var local_query = _dbContext.Games
                .Include(g => g.Categories)
                .Include(g => g.Owners)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                local_query = local_query.Where(g => g.Name.Contains(search));
            }

            var local_totalCount = await local_query.CountAsync();
            var local_totalPages = (int)Math.Ceiling(local_totalCount / (double)local_limit);

            var local_games = await local_query
                .OrderByDescending(g => g.CreatedAt)
                .Skip(local_offset)
                .Take(local_limit)
                .Select(g => new AdminGameListItem
                {
                    Id = g.Id,
                    Name = g.Name,
                    Price = g.Price,
                    Size = g.Size,
                    CreatedAt = g.CreatedAt,
                    CategoryNames = g.Categories.Select(c => c.Name).ToList(),
                    OwnerCount = g.Owners.Count
                })
                .ToListAsync();

            var local_viewModel = new AdminGamesViewModel
            {
                Games = local_games,
                TotalCount = local_totalCount,
                CurrentPage = page,
                TotalPages = local_totalPages,
                SearchQuery = search
            };

            return View(local_viewModel);
        }

        // GET: /Admin/CreateGame
        [HttpGet]
        public async Task<IActionResult> CreateGame()
        {
            var local_categories = await GetAvailableCategories();
            var local_viewModel = new AdminCreateGameViewModel
            {
                AvailableCategories = local_categories
            };
            return View(local_viewModel);
        }

        // POST: /Admin/CreateGame
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGame(AdminCreateGameViewModel model)
        {
            model.AvailableCategories = await GetAvailableCategories();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                model.Error = "Game name is required";
                return View(model);
            }

            var local_game = new Game
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                Price = model.Price,
                CreatedAt = DateTime.UtcNow
            };

            // Handle game file upload
            if (model.GameFile != null && model.GameFile.Length > 0)
            {
                using var local_ms = new MemoryStream();
                await model.GameFile.CopyToAsync(local_ms);
                local_game.Payload = local_ms.ToArray();
                local_game.FileName = model.GameFile.FileName;
                local_game.Size = model.GameFile.Length;
            }

            // Add categories
            if (model.SelectedCategoryIds.Any())
            {
                var local_categories = await _dbContext.Categories
                    .Where(c => model.SelectedCategoryIds.Contains(c.Id))
                    .ToListAsync();
                local_game.Categories = local_categories;
            }

            _dbContext.Games.Add(local_game);
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = $"Game \"{local_game.Name}\" created successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Games");
        }

        // GET: /Admin/EditGame/5
        [HttpGet]
        public async Task<IActionResult> EditGame(int id)
        {
            var local_game = await _dbContext.Games
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (local_game == null)
            {
                TempData["Message"] = "Game not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Games");
            }

            var local_viewModel = new AdminEditGameViewModel
            {
                Id = local_game.Id,
                Name = local_game.Name,
                Description = local_game.Description,
                Price = local_game.Price,
                CurrentFileName = local_game.FileName,
                CurrentSize = local_game.Size,
                SelectedCategoryIds = local_game.Categories.Select(c => c.Id).ToList(),
                AvailableCategories = await GetAvailableCategories()
            };

            return View(local_viewModel);
        }

        // POST: /Admin/EditGame/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGame(int id, AdminEditGameViewModel model)
        {
            model.AvailableCategories = await GetAvailableCategories();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                model.Error = "Game name is required";
                return View(model);
            }

            var local_game = await _dbContext.Games
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (local_game == null)
            {
                TempData["Message"] = "Game not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Games");
            }

            local_game.Name = model.Name;
            local_game.Description = model.Description ?? string.Empty;
            local_game.Price = model.Price;

            // Handle game file upload
            if (model.GameFile != null && model.GameFile.Length > 0)
            {
                using var local_ms = new MemoryStream();
                await model.GameFile.CopyToAsync(local_ms);
                local_game.Payload = local_ms.ToArray();
                local_game.FileName = model.GameFile.FileName;
                local_game.Size = model.GameFile.Length;
            }

            // Update categories
            local_game.Categories.Clear();
            if (model.SelectedCategoryIds.Any())
            {
                var local_categories = await _dbContext.Categories
                    .Where(c => model.SelectedCategoryIds.Contains(c.Id))
                    .ToListAsync();
                foreach (var local_cat in local_categories)
                {
                    local_game.Categories.Add(local_cat);
                }
            }

            await _dbContext.SaveChangesAsync();

            TempData["Message"] = $"Game \"{local_game.Name}\" updated successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Games");
        }

        // POST: /Admin/DeleteGame/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var local_game = await _dbContext.Games
                .Include(g => g.Categories)
                .Include(g => g.Owners)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (local_game == null)
            {
                TempData["Message"] = "Game not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Games");
            }

            var local_gameName = local_game.Name;
            local_game.Categories.Clear();
            local_game.Owners.Clear();
            _dbContext.Games.Remove(local_game);
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = $"Game \"{local_gameName}\" deleted";
            TempData["MessageType"] = "success";
            return RedirectToAction("Games");
        }

        #endregion

        #region Category Management

        // GET: /Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var local_categories = await _dbContext.Categories
                .Include(c => c.Games)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    GameCount = c.Games.Count
                })
                .ToListAsync();

            var local_viewModel = new AdminCategoriesViewModel
            {
                Categories = local_categories
            };

            return View(local_viewModel);
        }

        // GET: /Admin/CreateCategory
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new AdminCreateCategoryViewModel());
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(AdminCreateCategoryViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                model.Error = "Category name is required";
                return View(model);
            }

            // Check for duplicates
            var local_exists = await _dbContext.Categories.AnyAsync(c => c.Name == model.Name);
            if (local_exists)
            {
                model.Error = "Category name already exists";
                return View(model);
            }

            var local_category = new Category
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty
            };

            _dbContext.Categories.Add(local_category);
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = $"Category \"{local_category.Name}\" created successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Categories");
        }

        // GET: /Admin/EditCategory/5
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var local_category = await _dbContext.Categories
                .Include(c => c.Games)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (local_category == null)
            {
                TempData["Message"] = "Category not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Categories");
            }

            var local_viewModel = new AdminEditCategoryViewModel
            {
                Id = local_category.Id,
                Name = local_category.Name,
                Description = local_category.Description,
                GameCount = local_category.Games.Count
            };

            return View(local_viewModel);
        }

        // POST: /Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, AdminEditCategoryViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                model.Error = "Category name is required";
                return View(model);
            }

            var local_category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (local_category == null)
            {
                TempData["Message"] = "Category not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Categories");
            }

            // Check for duplicates (exclude self)
            var local_exists = await _dbContext.Categories
                .AnyAsync(c => c.Name == model.Name && c.Id != id);
            if (local_exists)
            {
                model.Error = "Category name already exists";
                return View(model);
            }

            local_category.Name = model.Name;
            local_category.Description = model.Description ?? string.Empty;
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = $"Category \"{local_category.Name}\" updated successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Categories");
        }

        // POST: /Admin/DeleteCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var local_category = await _dbContext.Categories
                .Include(c => c.Games)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (local_category == null)
            {
                TempData["Message"] = "Category not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Categories");
            }

            if (local_category.Games.Any())
            {
                TempData["Message"] = $"Category \"{local_category.Name}\" still has {local_category.Games.Count} games, cannot delete";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Categories");
            }

            var local_categoryName = local_category.Name;
            _dbContext.Categories.Remove(local_category);
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = $"Category \"{local_categoryName}\" deleted";
            TempData["MessageType"] = "success";
            return RedirectToAction("Categories");
        }

        #endregion

        #region User Management

        // GET: /Admin/Users
        public async Task<IActionResult> Users(int page = 1, string? search = null)
        {
            var local_limit = 15;
            var local_offset = (page - 1) * local_limit;

            var local_query = _dbContext.Users
                .Include(u => u.OwnedGames)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                local_query = local_query.Where(u =>
                    (u.Email != null && u.Email.Contains(search)) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search));
            }

            var local_totalCount = await local_query.CountAsync();
            var local_totalPages = (int)Math.Ceiling(local_totalCount / (double)local_limit);

            var local_users = await local_query
                .OrderByDescending(u => u.RegisteredAt)
                .Skip(local_offset)
                .Take(local_limit)
                .Select(u => new AdminUserListItem
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    RegisteredAt = u.RegisteredAt,
                    OwnedGameCount = u.OwnedGames.Count
                })
                .ToListAsync();

            // Mark admins
            foreach (var local_user in local_users)
            {
                var local_identityUser = await _userManager.FindByIdAsync(local_user.Id);
                if (local_identityUser != null)
                {
                    local_user.IsAdmin = await _userManager.IsInRoleAsync(local_identityUser, "Admin");
                }
            }

            var local_viewModel = new AdminUsersViewModel
            {
                Users = local_users,
                TotalCount = local_totalCount,
                CurrentPage = page,
                TotalPages = local_totalPages,
                SearchQuery = search
            };

            return View(local_viewModel);
        }

        // GET: /Admin/UserDetail/{id}
        public async Task<IActionResult> UserDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Users");
            }

            var local_user = await _dbContext.Users
                .Include(u => u.OwnedGames)
                    .ThenInclude(g => g.Categories)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (local_user == null)
            {
                TempData["Message"] = "User not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Users");
            }

            var local_isAdmin = await _userManager.IsInRoleAsync(local_user, "Admin");

            var local_viewModel = new AdminUserDetailViewModel
            {
                Id = local_user.Id,
                Email = local_user.Email ?? string.Empty,
                FirstName = local_user.FirstName,
                LastName = local_user.LastName,
                RegisteredAt = local_user.RegisteredAt,
                IsAdmin = local_isAdmin,
                OwnedGames = local_user.OwnedGames.Select(g => new GameListDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Price = g.Price,
                    Size = g.Size,
                    CreatedAt = g.CreatedAt,
                    CategoryNames = g.Categories.Select(c => c.Name).ToList(),
                    IsOwned = true
                }).ToList()
            };

            return View(local_viewModel);
        }

        // POST: /Admin/ToggleAdmin/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string id)
        {
            var local_user = await _userManager.FindByIdAsync(id);
            if (local_user == null)
            {
                TempData["Message"] = "User not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Users");
            }

            var local_currentUser = await _userManager.GetUserAsync(User);
            if (local_currentUser?.Id == id)
            {
                TempData["Message"] = "Cannot modify your own admin privileges";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Users");
            }

            var local_isAdmin = await _userManager.IsInRoleAsync(local_user, "Admin");
            if (local_isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(local_user, "Admin");
                TempData["Message"] = $"Admin privileges removed for {local_user.Email}";
            }
            else
            {
                await _userManager.AddToRoleAsync(local_user, "Admin");
                TempData["Message"] = $"Admin privileges granted to {local_user.Email}";
            }
            TempData["MessageType"] = "success";

            return RedirectToAction("Users");
        }

        #endregion

        #region Helper Methods

        private async Task<List<CategoryDto>> GetAvailableCategories()
        {
            return await _dbContext.Categories
                .Include(c => c.Games)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    GameCount = c.Games.Count
                })
                .ToListAsync();
        }

        #endregion
    }
}
