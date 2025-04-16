using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.API.Model;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Infrastructure.Services;

namespace PersonalFinanceManager.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GmailApiController : ControllerBase
    {
        private readonly IGmailService _gmailService;
        private readonly TransactionService _transactionService;
        private readonly GmailServiceSettings _gmailSettings;

        private readonly string _credentialPath = "credentials.json";

        public GmailApiController(IGmailService gmailService,
                               TransactionService transactionService,
                               IOptions<GmailServiceSettings> gmailOptions)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
            _gmailSettings = gmailOptions.Value;
        }

        [HttpGet]
        public async Task<IActionResult> InitiateOAuth()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var redirectUri = "http://localhost:8000/api/GmailApi/callback";

            var authUrl = await _gmailService.InitiateOAuthFlowAsync(userId,_credentialPath, redirectUri);

            return Ok(new { AuthUrl = authUrl });
        }

        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string code, string state, string error)
        {
            try
            {
                if (!string.IsNullOrEmpty(error))
                {
                    // Chuyển hướng về MVC với thông báo lỗi
                    return Redirect($"https://localhost:7204/GmailManagement/callback?error={Uri.EscapeDataString(error)}");
                }

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    return Redirect("https://localhost:7204/GmailManagement/callback?error=Invalid code or state");
                }

                try
                {
                    var redirectUri = "http://localhost:8000/api/gmailApi/callback";
                    await _gmailService.ExchangeCodeForTokenAsync(code, redirectUri, state);
                    // Chuyển hướng về MVC với trạng thái thành công
                    return Redirect("https://localhost:7204/GmailManagement/callback?success=true");
                }
                catch (Exception ex)
                {
                    // Chuyển hướng về MVC với thông báo lỗi
                    return Redirect($"http://localhost:8000/api/gmailApi/callback?error={Uri.EscapeDataString(ex.Message)}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}