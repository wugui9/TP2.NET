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
    /// 商店前端控制器 - 将PHP客户端转换为C# MVC视图
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

        #region 辅助方法

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

        // 格式化文件大小
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

        // 格式化价格
        public static string FormatPrice(decimal price)
        {
            return $"{price:F2} €";
        }

        // 格式化日期
        public static string FormatDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }

        #endregion

        #region 首页

        // GET: /Store
        public async Task<IActionResult> Index()
        {
            var local_currentUserId = await GetCurrentUserIdAsync();

            // 获取最新6款游戏
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

            // 获取所有分类
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

        #region 游戏列表

        // GET: /Store/Games
        public async Task<IActionResult> Games(int page = 1)
        {
            var local_limit = 12;
            var local_offset = (page - 1) * local_limit;
            var local_currentUserId = await GetCurrentUserIdAsync();

            var local_query = _dbContext.Games
                .Include(g => g.Categories)
                .Include(g => g.Owners)
                .AsQueryable();

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
                IsLoggedIn = IsLoggedIn()
            };

            return View(local_viewModel);
        }

        #endregion

        #region 游戏详情

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
                TempData["Message"] = "游戏不存在";
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
                TempData["Message"] = "请先登录";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Login");
            }

            var local_game = await _dbContext.Games
                .Include(g => g.Owners)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (local_game == null)
            {
                TempData["Message"] = "游戏不存在";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Games");
            }

            var local_user = await _userManager.GetUserAsync(User);
            if (local_user == null) return RedirectToAction("Login");

            if (local_game.Owners.Any(o => o.Id == local_user.Id))
            {
                TempData["Message"] = "您已经拥有此游戏";
                TempData["MessageType"] = "warning";
            }
            else
            {
                local_game.Owners.Add(local_user);
                await _dbContext.SaveChangesAsync();
                TempData["Message"] = "购买成功！";
                TempData["MessageType"] = "success";
            }

            return RedirectToAction("Game", new { id });
        }

        #endregion

        #region 分类

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
                TempData["Message"] = "分类不存在";
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

        #region 游戏库

        // GET: /Store/Library
        public async Task<IActionResult> Library()
        {
            if (!IsLoggedIn())
            {
                TempData["Message"] = "请先登录以查看您的游戏库";
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
                UserName = local_user.FirstName ?? local_user.Email ?? "用户",
                TotalSize = local_ownedGames.Sum(g => g.Size),
                TotalValue = local_ownedGames.Sum(g => g.Price)
            };

            return View(local_viewModel);
        }

        #endregion

        #region 个人信息

        // GET: /Store/Profile
        public async Task<IActionResult> Profile()
        {
            if (!IsLoggedIn())
            {
                TempData["Message"] = "请先登录";
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

        #region 登录/注册/登出

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
                model.Error = "请填写所有字段";
                return View(model);
            }

            var local_user = await _userManager.FindByEmailAsync(model.Email);
            if (local_user == null)
            {
                model.Error = "邮箱或密码错误";
                return View(model);
            }

            var local_result = await _signInManager.PasswordSignInAsync(
                local_user.UserName ?? local_user.Email!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!local_result.Succeeded)
            {
                model.Error = "邮箱或密码错误";
                return View(model);
            }

            // 保存session信息
            HttpContext.Session.SetString("UserId", local_user.Id);
            HttpContext.Session.SetString("UserEmail", local_user.Email ?? string.Empty);

            TempData["Message"] = $"登录成功！欢迎回来 {local_user.FirstName}";
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
                model.Error = "请填写所有字段";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                model.Error = "两次输入的密码不一致";
                return View(model);
            }

            if (model.Password.Length < 6)
            {
                model.Error = "密码长度至少为6个字符";
                return View(model);
            }

            var local_existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (local_existingUser != null)
            {
                model.Error = "该邮箱已被注册";
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
                model.Error = "注册失败: " + string.Join(", ", local_result.Errors.Select(e => e.Description));
                return View(model);
            }

            TempData["Message"] = "注册成功！请登录";
            TempData["MessageType"] = "success";

            return RedirectToAction("Login");
        }

        // GET: /Store/Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();

            TempData["Message"] = "已成功退出登录";
            TempData["MessageType"] = "info";

            return RedirectToAction("Index");
        }

        #endregion

        #region 下载

        // GET: /Store/Download/5
        public async Task<IActionResult> Download(int id)
        {
            if (!IsLoggedIn())
            {
                TempData["Message"] = "请先登录";
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
                TempData["Message"] = "游戏不存在";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Library");
            }

            var local_currentUserId = await GetCurrentUserIdAsync();
            if (local_currentUserId == null || !local_game.Owners.Any(o => o.Id == local_currentUserId))
            {
                TempData["Message"] = "您没有此游戏或游戏不存在";
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

            // 如果没有文件数据，返回示例文件
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
