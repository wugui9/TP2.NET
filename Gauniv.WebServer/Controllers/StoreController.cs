using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Gauniv.WebServer.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Gauniv.WebServer.Controllers
{
    /// <summary>
    /// Store frontend controller
    /// </summary>
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;

        public StoreController(
            ApplicationDbContext dbContext,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        #region Helper Methods

        private async Task<string?> GetCurrentUserIdAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var local_user = await _userManager.GetUserAsync(User);
                return local_user?.Id;
            }
            return null;
        }

        private bool IsLoggedIn() => User.Identity?.IsAuthenticated == true;

        // Format file size
        public static string FormatSize(long bytes)
        {
            if (bytes >= 1073741824)
                return $"{bytes / 1073741824.0:F2} GB";
            if (bytes >= 1048576)
                return $"{bytes / 1048576.0:F2} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F2} KB";
            return $"{bytes} B";
        }

        // Format price
        public static string FormatPrice(decimal price)
        {
            return $"{price:F2} €";
        }

        // Format date
        public static string FormatDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }

        #endregion

        #region Home

        // GET: /Store
        public async Task<IActionResult> Index()
        {
            var local_currentUserId = await GetCurrentUserIdAsync();

            // Get latest 6 games
            var local_latestGames = await _dbContext.Games
                .Include(g => g.Categories)
                .Include(g => g.Owners)
                .OrderByDescending(g => g.CreatedAt)
                .Take(6)
                .Select(g => new GameListDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Price = g.Price,
                    Size = g.Size,
                    CreatedAt = g.CreatedAt,
                    CategoryNames = g.Categories.Select(c => c.Name).ToList(),
                    IsOwned = local_currentUserId != null && g.Owners.Any(o => o.Id == local_currentUserId)
                })
                .ToListAsync();

            // Get all categories
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

            var local_viewModel = new StoreIndexViewModel
            {
                LatestGames = local_latestGames,
                Categories = local_categories
            };

            return View(local_viewModel);
        }

        #endregion

        #region Game List

        // GET: /Store/Games?page=1&categoryId=&minPrice=&maxPrice=&owned=
        public async Task<IActionResult> Games(int page = 1, int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null, bool? owned = null)
        {
            var local_limit = 12;
            var local_offset = (page - 1) * local_limit;
            var local_currentUserId = await GetCurrentUserIdAsync();

            var local_query = _dbContext.Games
                .Include(g => g.Categories)
                .Include(g => g.Owners)
                .AsQueryable();

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                local_query = local_query.Where(g => g.Categories.Any(c => c.Id == categoryId.Value));
            }

            // Filter by price
            if (minPrice.HasValue)
            {
                local_query = local_query.Where(g => g.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                local_query = local_query.Where(g => g.Price <= maxPrice.Value);
            }

            // Filter by ownership
            if (owned == true && local_currentUserId != null)
            {
                local_query = local_query.Where(g => g.Owners.Any(o => o.Id == local_currentUserId));
            }

            var local_totalCount = await local_query.CountAsync();
            var local_totalPages = (int)Math.Ceiling(local_totalCount / (double)local_limit);

            var local_games = await local_query
                .OrderByDescending(g => g.CreatedAt)
                .Skip(local_offset)
                .Take(local_limit)
                .Select(g => new GameListDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Price = g.Price,
                    Size = g.Size,
                    CreatedAt = g.CreatedAt,
                    CategoryNames = g.Categories.Select(c => c.Name).ToList(),
                    IsOwned = local_currentUserId != null && g.Owners.Any(o => o.Id == local_currentUserId)
                })
                .ToListAsync();

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

            var local_viewModel = new StoreGamesViewModel
            {
                Games = local_games,
                Categories = local_categories,
                TotalCount = local_totalCount,
                CurrentPage = page,
                TotalPages = local_totalPages,
                Limit = local_limit,
                IsLoggedIn = IsLoggedIn(),
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Owned = owned
            };

            return View(local_viewModel);
        }

        #endregion

        #region Game Details

        // GET: /Store/Game/5
        public async Task<IActionResult> Game(int id)
        {
            if (id <= 0) return RedirectToAction("Games");

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

            var local_currentUserId = await GetCurrentUserIdAsync();
            var local_gameDto = _mapper.Map<GameDto>(local_game);
            local_gameDto.IsOwned = local_currentUserId != null && local_game.Owners.Any(o => o.Id == local_currentUserId);

            var local_viewModel = new StoreGameDetailViewModel
            {
                Game = local_gameDto,
                IsLoggedIn = IsLoggedIn()
            };

            return View(local_viewModel);
        }

        // POST: /Store/Purchase/5
        [HttpPost]
        public async Task<IActionResult> Purchase(int id)
        {
            if (!IsLoggedIn())
            {
                TempData["Message"] = "Please login first";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Login");
            }

            var local_game = await _dbContext.Games
                .Include(g => g.Owners)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (local_game == null)
            {
                TempData["Message"] = "Game not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Games");
            }

            var local_user = await _userManager.GetUserAsync(User);
            if (local_user == null) return RedirectToAction("Login");

            if (local_game.Owners.Any(o => o.Id == local_user.Id))
            {
                TempData["Message"] = "You already own this game";
                TempData["MessageType"] = "warning";
            }
            else
            {
                local_game.Owners.Add(local_user);
                await _dbContext.SaveChangesAsync();
                TempData["Message"] = "Purchase successful!";
                TempData["MessageType"] = "success";
            }

            return RedirectToAction("Game", new { id });
        }

        #endregion

        #region Categories

        // GET: /Store/Categories
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

            var local_viewModel = new StoreCategoriesViewModel
            {
                Categories = local_categories
            };

            return View(local_viewModel);
        }

        // GET: /Store/Category/5
        public async Task<IActionResult> Category(int id)
        {
            if (id <= 0) return RedirectToAction("Categories");

            var local_category = await _dbContext.Categories
                .Include(c => c.Games)
                    .ThenInclude(g => g.Categories)
                .Include(c => c.Games)
                    .ThenInclude(g => g.Owners)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (local_category == null)
            {
                TempData["Message"] = "Category not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Categories");
            }

            var local_currentUserId = await GetCurrentUserIdAsync();

            var local_viewModel = new StoreCategoryDetailViewModel
            {
                Category = new CategoryDto
                {
                    Id = local_category.Id,
                    Name = local_category.Name,
                    Description = local_category.Description,
                    GameCount = local_category.Games.Count
                },
                Games = local_category.Games
                    .OrderByDescending(g => g.CreatedAt)
                    .Select(g => new GameListDto
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Price = g.Price,
                        Size = g.Size,
                        CreatedAt = g.CreatedAt,
                        CategoryNames = g.Categories.Select(c => c.Name).ToList(),
                        IsOwned = local_currentUserId != null && g.Owners.Any(o => o.Id == local_currentUserId)
                    })
                    .ToList()
            };

            return View(local_viewModel);
        }

        #endregion

        #region Library

        // GET: /Store/Library
        public async Task<IActionResult> Library()
        {
            if (!IsLoggedIn())
            {
                TempData["Message"] = "Please login to view your library";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Login");
            }

            var local_user = await _userManager.GetUserAsync(User);
            if (local_user == null) return RedirectToAction("Login");

            var local_ownedGames = await _dbContext.Games
                .Include(g => g.Categories)
                .Include(g => g.Owners)
                .Where(g => g.Owners.Any(o => o.Id == local_user.Id))
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new GameListDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Price = g.Price,
                    Size = g.Size,
                    CreatedAt = g.CreatedAt,
                    CategoryNames = g.Categories.Select(c => c.Name).ToList(),
                    IsOwned = true
                })
                .ToListAsync();

            var local_viewModel = new StoreLibraryViewModel
            {
                OwnedGames = local_ownedGames,
                UserName = local_user.FirstName ?? local_user.Email ?? "User",
                TotalSize = local_ownedGames.Sum(g => g.Size),
                TotalValue = local_ownedGames.Sum(g => g.Price)
            };

            return View(local_viewModel);
        }

        #endregion

        #region Profile

        // GET: /Store/Profile
        public async Task<IActionResult> Profile()
        {
            if (!IsLoggedIn())
            {
                TempData["Message"] = "Please login first";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Login");
            }

            var local_user = await _userManager.GetUserAsync(User);
            if (local_user == null) return RedirectToAction("Login");

            var local_viewModel = new StoreProfileViewModel
            {
                Email = local_user.Email ?? string.Empty,
                FirstName = local_user.FirstName,
                LastName = local_user.LastName,
                RegisteredAt = local_user.RegisteredAt
            };

            return View(local_viewModel);
        }

        #endregion

        #region Login/Register/Logout

        // GET: /Store/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (IsLoggedIn()) return RedirectToAction("Index");
            return View(new StoreLoginViewModel());
        }

        // POST: /Store/Login
        [HttpPost]
        public async Task<IActionResult> Login(StoreLoginViewModel model)
        {
            if (IsLoggedIn()) return RedirectToAction("Index");

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                model.Error = "Please fill in all fields";
                return View(model);
            }

            var local_user = await _userManager.FindByEmailAsync(model.Email);
            if (local_user == null)
            {
                model.Error = "Invalid email or password";
                return View(model);
            }

            var local_result = await _signInManager.PasswordSignInAsync(
                local_user.UserName ?? local_user.Email!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!local_result.Succeeded)
            {
                model.Error = "Invalid email or password";
                return View(model);
            }

            // Save session info
            HttpContext.Session.SetString("UserId", local_user.Id);
            HttpContext.Session.SetString("UserEmail", local_user.Email ?? string.Empty);

            // Redirect admin to admin panel after login
            if (await _userManager.IsInRoleAsync(local_user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: /Store/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (IsLoggedIn()) return RedirectToAction("Index");
            return View(new StoreRegisterViewModel());
        }

        // POST: /Store/Register
        [HttpPost]
        public async Task<IActionResult> Register(StoreRegisterViewModel model)
        {
            if (IsLoggedIn()) return RedirectToAction("Index");

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password) ||
                string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
            {
                model.Error = "Please fill in all fields";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                model.Error = "Passwords do not match";
                return View(model);
            }

            var local_existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (local_existingUser != null)
            {
                model.Error = "This email is already registered";
                return View(model);
            }

            var local_newUser = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegisteredAt = DateTime.UtcNow,
                PlainPassword = model.Password
            };

            var local_result = await _userManager.CreateAsync(local_newUser, model.Password);

            if (!local_result.Succeeded)
            {
                model.Error = "Registration failed: " + string.Join(", ", local_result.Errors.Select(e => e.Description));
                return View(model);
            }

            TempData["Message"] = "Registration successful! Please login";
            TempData["MessageType"] = "success";

            return RedirectToAction("Login");
        }

        // GET: /Store/Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();

            TempData["Message"] = "You have been logged out";
            TempData["MessageType"] = "info";

            return RedirectToAction("Index");
        }

        #endregion

        #region Download

        // GET: /Store/Download/5
        public async Task<IActionResult> Download(int id)
        {
            if (!IsLoggedIn())
            {
                TempData["Message"] = "Please login first";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Login");
            }

            if (id <= 0) return RedirectToAction("Library");

            var local_game = await _dbContext.Games
                .Include(g => g.Categories)
                .Include(g => g.Owners)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (local_game == null)
            {
                TempData["Message"] = "Game not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Library");
            }

            var local_currentUserId = await GetCurrentUserIdAsync();
            if (local_currentUserId == null || !local_game.Owners.Any(o => o.Id == local_currentUserId))
            {
                TempData["Message"] = "You do not own this game or it does not exist";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Library");
            }

            var local_gameDto = _mapper.Map<GameDto>(local_game);
            local_gameDto.IsOwned = true;

            var local_viewModel = new StoreDownloadViewModel
            {
                Game = local_gameDto,
                GameId = id
            };

            return View(local_viewModel);
        }

        // GET: /Store/StartDownload/5
        public async Task<IActionResult> StartDownload(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var local_game = await _dbContext.Games
                .Include(g => g.Owners)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (local_game == null) return NotFound();

            var local_currentUserId = await GetCurrentUserIdAsync();
            if (local_currentUserId == null || !local_game.Owners.Any(o => o.Id == local_currentUserId))
            {
                return Forbid();
            }

            // Return demo file if no file data
            if (local_game.Payload == null || local_game.Payload.Length == 0)
            {
                var local_demoContent = Encoding.UTF8.GetBytes($"Demo game: {local_game.Name}\nThis is a test game file.");
                return File(local_demoContent, "application/octet-stream", local_game.FileName ?? $"{local_game.Name}.txt");
            }

            return File(local_game.Payload, "application/octet-stream", local_game.FileName ?? $"{local_game.Name}.exe");
        }

        #endregion
    }
}
