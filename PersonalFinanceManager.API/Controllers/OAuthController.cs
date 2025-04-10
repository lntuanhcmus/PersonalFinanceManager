using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Services;

namespace PersonalFinanceManager.Controllers
{
    [Route("oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly GmailService _gmailService;

        public OAuthController(GmailService gmailService)
        {
            _gmailService = gmailService;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> OAuthCallback(string code)
        {
            try
            {
                var credential = await _gmailService.ExchangeCodeForTokenAsync(code);
                await _gmailService.ExtractTransactionsAsync("credentials.json","token.json");
                return Ok("Token đã được lưu và giao dịch đã trích xuất.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}