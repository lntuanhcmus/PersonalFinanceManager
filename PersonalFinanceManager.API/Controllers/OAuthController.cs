using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Services;
using System.Transactions;

namespace PersonalFinanceManager.Controllers
{
    [Route("oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly GmailService _gmailService;
        private readonly TransactionService _transactionService;

        public OAuthController(GmailService gmailService, TransactionService transactionService)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> OAuthCallback(string code)
        {
            try
            {
                var credential = await _gmailService.ExchangeCodeForTokenAsync(code);
                var transaction = await _gmailService.ExtractTransactionsAsync("credentials.json","token.json");
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