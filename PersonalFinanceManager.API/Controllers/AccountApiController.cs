using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Infrastructure.Repositories;
using PersonalFinanceManager.Infrastructure.Services;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Dto;
using System.Security.Claims;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PersonalFinanceManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUserTokenService _userTokenService;
        private readonly IExternalTokenService _externalTokenService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountApiController> _logger;

        public AccountApiController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IUserTokenService userTokenService,
            IExternalTokenService externalTokenService,
            IUserRepository userRepository,
            ILogger<AccountApiController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userTokenService = userTokenService;
            _externalTokenService = externalTokenService;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new AppUser
            {
                UserName = model.Email, 
                Email = model.Email,
                FullName = model.FullName,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = model.PhoneNumber,
                PreferredCurrency = model.PreferredCurrency,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
                return StatusCode(201, "User registered successfully.");

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // Log khi request đi vào action
            _logger.LogInformation("Login request received for email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for login request: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login failed: Invalid email or account inactive for email: {Email}", model.Email);
                return Unauthorized("Invalid email or account is inactive.");
            }

            // Sử dụng SignInManager để đăng nhập
            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? user.Email, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Cập nhật LastLogin
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Tạo token JWT
                var token = await _userTokenService.GenerateTokenAsync(user);
                _logger.LogInformation("Login successful for email: {Email}", model.Email);
                return Ok(token);
            }

            _logger.LogWarning("Login failed: Invalid email or password for email: {Email}", model.Email);
            return Unauthorized("Invalid email or password.");
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token.");

            var isValid = await _userTokenService.ValidateRefreshTokenAsync(refreshToken, userId);
            if (!isValid)
                return Unauthorized("Invalid or expired refresh token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found.");

            await _userTokenService.RevokeRefreshTokenAsync(refreshToken);
            var newToken = await _userTokenService.GenerateTokenAsync(user);
            return Ok(newToken);
        }

        [HttpGet("info")]
        [Authorize]
        public async Task<ActionResult<UserInfoDto>> GetUserInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userInfo = (await _userManager.FindByIdAsync(userId));
            var result = new UserInfoDto()
            {
                Id = userInfo.Id,
                Email = userInfo.Email,
                Name = userInfo.FullName,
                IsGmailConnected = userInfo.IsConnectedGmail,
            };

            return Ok(result);
        }

        [HttpPost("disconnect-gmail")]
        [Authorize]
        public async Task<IActionResult> DisconnectGmail()
        {
            try
            {
                int userId = Int16.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = (await _userRepository.FindByIdAsync(userId));

                user.IsConnectedGmail = false;

                await _userRepository.SaveChangeAsync(user);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("get-integrations")]
        [Authorize]
        public async Task<ActionResult<ExternalIntegrationsDto>> GetIntegrations()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = (await _userManager.FindByIdAsync(userId));

                var result = new ExternalIntegrationsDto() { IsConnectedGmail = user.IsConnectedGmail, };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi gọi API: {ex.Message}" });
            }
        }

    }
}
