#region Header
#endregion

using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Gauniv.WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(
            ApplicationDbContext context,
            ILogger<AuthController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// 登录 - 使用 Identity 验证
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 根据 Email 查找用户
            var local_user = await _userManager.FindByEmailAsync(request.Email);

            if (local_user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // 使用 SignInManager 验证密码
            var local_result = await _signInManager.PasswordSignInAsync(
                local_user.UserName ?? local_user.Email,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: false);

            if (!local_result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            _logger.LogInformation($"User {request.Email} logged in successfully");

            // 在 Session 中保存用户信息
            HttpContext.Session.SetString("UserId", local_user.Id);
            HttpContext.Session.SetString("UserEmail", local_user.Email ?? string.Empty);

            return Ok(new LoginResponseDto
            {
                Success = true,
                Email = local_user.Email ?? string.Empty,
                FirstName = local_user.FirstName,
                LastName = local_user.LastName,
                Message = "Login successful"
            });
        }

        /// <summary>
        /// 登出
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return Ok(new { message = "Logout successful" });
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var local_userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(local_userId))
            {
                return Unauthorized();
            }

            var local_user = await _context.Users.FindAsync(local_userId);
            if (local_user == null)
            {
                return NotFound();
            }

            return Ok(new UserInfoDto
            {
                Email = local_user.Email ?? string.Empty,
                FirstName = local_user.FirstName,
                LastName = local_user.LastName,
                RegisteredAt = local_user.RegisteredAt
            });
        }

        /// <summary>
        /// 注册新用户
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 检查邮箱是否已存在
            var local_existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (local_existingUser != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // 创建新用户
            var local_newUser = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                RegisteredAt = DateTime.UtcNow,
                PlainPassword = request.Password // 可选：保存明文密码（不推荐）
            };

            // 使用 UserManager 创建用户（会自动哈希密码）
            var local_result = await _userManager.CreateAsync(local_newUser, request.Password);

            if (!local_result.Succeeded)
            {
                return BadRequest(new { 
                    message = "Registration failed",
                    errors = local_result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation($"New user registered: {request.Email}");

            return Ok(new { message = "Registration successful" });
        }
    }

    // DTOs for Auth
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
    }

    public class UserInfoDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}
