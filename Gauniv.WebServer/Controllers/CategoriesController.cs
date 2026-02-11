#region Header
#endregion

using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gauniv.WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ApplicationDbContext dbContext,
            ILogger<CategoriesController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有游戏类别
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var local_categories = await _dbContext.Categories
                .Include(c => c.Games)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var local_categoryDtos = local_categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                GameCount = c.Games.Count
            }).ToList();

            return Ok(local_categoryDtos);
        }

        /// <summary>
        /// 根据ID获取类别详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var local_category = await _dbContext.Categories
                .Include(c => c.Games)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (local_category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            var local_categoryDto = new CategoryDto
            {
                Id = local_category.Id,
                Name = local_category.Name,
                Description = local_category.Description,
                GameCount = local_category.Games.Count
            };

            return Ok(local_categoryDto);
        }

        /// <summary>
        /// 获取某个类别下的所有游戏
        /// </summary>
        [HttpGet("{id}/games")]
        public async Task<IActionResult> GetGamesByCategory(int id)
        {
            var local_category = await _dbContext.Categories
                .Include(c => c.Games)
                    .ThenInclude(g => g.Categories)
                .Include(c => c.Games)
                    .ThenInclude(g => g.Owners)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (local_category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            var local_gameDtos = local_category.Games
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new GameListDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Price = g.Price,
                    Size = g.Size,
                    CreatedAt = g.CreatedAt,
                    CategoryNames = g.Categories.Select(c => c.Name).ToList(),
                    IsOwned = false // 需要用户信息才能判断，这里简化处理
                }).ToList();

            return Ok(new
            {
                Category = new CategoryDto
                {
                    Id = local_category.Id,
                    Name = local_category.Name,
                    Description = local_category.Description,
                    GameCount = local_category.Games.Count
                },
                Games = local_gameDtos
            });
        }
    }
}
