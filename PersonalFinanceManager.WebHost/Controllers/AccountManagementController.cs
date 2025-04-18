using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Dto;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
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
                JsonConvert.SerializeObject(model),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("api/accountApi/register", content);
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Reauthorize(string? successMessage)
        {
            TempData["Success"] = successMessage;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Reauthorize()
        {
            var successMessage = TempData["Success"] ?? "SuccessMessage";
            return RedirectToAction("Index", "TransactionsManagement", new { SuccessMessage = successMessage });
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
                JsonConvert.SerializeObject(model),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("api/AccountApi/login", content);
            if (response.IsSuccessStatusCode)
            {
                var token = JsonConvert.DeserializeObject<TokenDto>(await response.Content.ReadAsStringAsync());
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
                    return RedirectToAction("Index", "TransactionsManagement");
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

        [HttpGet]
        public async Task<IActionResult> Info()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var response = await client.GetAsync("api/accountApi/info");
            if (response.IsSuccessStatusCode)
            {
                var userInfo = JsonConvert.DeserializeObject<UserInfoDto>(await response.Content.ReadAsStringAsync());
                return View(userInfo);
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> InitiateOAuth()
        {
            try
            {
                // Tạo client HTTP
                var client = _httpClientFactory.CreateClient("ApiClient");

                // Gửi yêu cầu tới API đích
                var response = await client.GetAsync("api/gmailApi/initiate-oauth");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorObj = JsonConvert.DeserializeObject<object>(json);
                    return BadRequest(new { error = errorObj?.ToString() ?? "Lỗi khi gọi API Gmail" });
                }

                // Phân tích kết quả từ API
                return Ok(json);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi gọi API: {ex.Message}" });
            }
            catch (JsonException)
            {
                return BadRequest(new { error = "Không thể phân tích phản hồi từ API" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DisconnectGmail()
        {
            try
            {
                // Tạo client HTTP
                var client = _httpClientFactory.CreateClient("ApiClient");

                var respose = await client.PostAsync("api/accountApi/disconnect-gmail", null);

                var json = await respose.Content.ReadAsStringAsync();

                if (!respose.IsSuccessStatusCode)
                {
                    var errorObj = JsonConvert.DeserializeObject<object>(json);
                    return BadRequest(new { error = errorObj?.ToString() ?? "Lỗi khi dừng kết nối Gmail" });
                }
                return Ok(json);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi gọi API: {ex.Message}" });
            }
            catch (JsonException)
            {
                return BadRequest(new { error = "Không thể phân tích phản hồi từ API" });
            }
        }
    }
}

