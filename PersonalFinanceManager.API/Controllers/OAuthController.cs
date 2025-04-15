using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.API.Model;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Infrastructure.Services;

namespace PersonalFinanceManager.Controllers
{
    [Route("oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IGmailService _gmailService;
        private readonly TransactionService _transactionService;
        private readonly GmailServiceSettings _gmailSettings;

        public OAuthController(IGmailService gmailService,
                               TransactionService transactionService,
                               IOptions<GmailServiceSettings> gmailOptions)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
            _gmailSettings = gmailOptions.Value;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> OAuthCallback(string code)
        {
            try
            {
                int maxResult = _gmailSettings.MaxResult;

                // Kiểm tra hợp lệ, nếu không dùng mặc định
                if (maxResult <= 0) maxResult = 10;
                var credential = await _gmailService.ExchangeCodeForTokenAsync(code);
                var transaction = await _gmailService.ExtractTransactionsAsync("credentials.json", maxResult, credential);
                if(transaction != null)
                {
                    await _transactionService.SaveTransactions(transaction);
                }
                return Ok("Token đã được lưu và giao dịch đã trích xuất.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}