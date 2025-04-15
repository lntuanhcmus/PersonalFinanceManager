using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Dto;
using System.Text;
using System.Text.Json;

namespace PersonalFinanceManager.WebHost.Controllers
{
    public class AccountManagementController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountManagementController(IHttpClientFactory httpClientFactory,
                                           IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterDto());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/account/register", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Registration failed: {errorContent}");
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginDto());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/account/login", content);
            if (response.IsSuccessStatusCode)
            {
                var token = JsonSerializer.Deserialize<TokenDto>(await response.Content.ReadAsStringAsync());
                if (token != null)
                {
                    // Lưu access token và refresh token vào cookie
                    Response.Cookies.Append("AccessToken", token.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn)
                    });

                    Response.Cookies.Append("RefreshToken", token.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa cookie
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
            TempData["SuccessMessage"] = "Logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}

