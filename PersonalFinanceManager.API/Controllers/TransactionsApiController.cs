using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Services;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.Shared.Dto;
using System.Globalization;

namespace PersonalFinanceManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsApiController : ControllerBase
    {
        private readonly GmailService _gmailService;
        //private readonly ExcelService _transactionService;
        private readonly TransactionService _transactionService;

        public TransactionsApiController(GmailService gmailService, TransactionService transactionService)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
        }

        //[HttpGet]
        //public ActionResult<List<Transaction>> Get()
        //{
        //    return Ok(_transactionService.GetTransactions());
        //}

        [HttpGet]
    public async Task<ActionResult<PagedResponse<Transaction>>> GetTransactions(
        string transactionId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        string category = null,
        string sourceAccount = null,
        string content = null,
        int page = 1,
        int pageSize = 10)
    {
        var transactions = _transactionService.GetTransactions();

        var filtered = transactions
            .Where(t => (string.IsNullOrEmpty(transactionId) || t.TransactionId.Contains(transactionId, StringComparison.OrdinalIgnoreCase)) &&
                        (!startDate.HasValue || t.TransactionTime >= startDate) &&
                        (!endDate.HasValue || t.TransactionTime <= endDate) &&
                        (!minAmount.HasValue || t.Amount >= minAmount) &&
                        (!maxAmount.HasValue || t.Amount <= maxAmount) &&
                        (string.IsNullOrEmpty(category) || t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(sourceAccount) || t.SourceAccount.Contains(sourceAccount, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(content) || t.Description.Contains(content, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var totalItems = filtered.Count;
        var pagedItems = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var response = new PagedResponse<Transaction>
        {
            Items = pagedItems,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(response);
    }

        [HttpGet("{id}")]
        public ActionResult<Transaction> GetById(string id)
        {
            var transaction = _transactionService.GetById(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshFromGmail()
        {
            var transactions = await _gmailService.ExtractTransactionsAsync("credentials.json", "token.json");
            await _transactionService.SaveTransactions(transactions);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] TransactionDto transactionDto)
        {
            if (transactionDto == null || string.IsNullOrEmpty(transactionDto.TransactionId))
                return BadRequest("Dữ liệu không hợp lệ");

            var transaction = new Transaction
            {
                TransactionId = transactionDto.TransactionId,
                TransactionTime = DateTime.ParseExact(transactionDto.TransactionTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                SourceAccount = transactionDto.SourceAccount,
                RecipientAccount = transactionDto.RecipientAccount,
                RecipientName = transactionDto.RecipientName,
                RecipientBank = transactionDto.RecipientBank,
                Amount = transactionDto.Amount,
                Description = transactionDto.Description,
                Category = transactionDto.Category
            };

            await _transactionService.AddTransaction(transaction);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(string id, [FromBody] TransactionDto transactionDto)
        {
            if (transactionDto == null || transactionDto.TransactionId != id)
                return BadRequest("Dữ liệu không hợp lệ");

            var transaction = _transactionService.GetById(id);
            if (transaction == null) return NotFound();
            transaction.TransactionId = transactionDto.TransactionId;
            transaction.TransactionTime = DateTime.ParseExact(transactionDto.TransactionTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            transaction.SourceAccount = transactionDto.SourceAccount;
            transaction.RecipientAccount = transactionDto.RecipientAccount;
            transaction.RecipientName = transactionDto.RecipientName;
            transaction.RecipientBank = transactionDto.RecipientBank;
            transaction.Amount = transactionDto.Amount;
            transaction.Description = transactionDto.Description;
            transaction.Category = transactionDto.Category;

            await _transactionService.UpdateTransaction(transaction);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var transaction = _transactionService.GetById(id);
            if (transaction == null)
                return NotFound();
            if (transaction.Category != "Nhận")
                return BadRequest("Chỉ có thể xóa giao dịch loại 'Nhận'");

            await _transactionService.DeleteTransaction(id); // Cần thêm hàm này
            return Ok();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<FinancialSummary>> GetFinancialSummary(DateTime? startDate, DateTime? endDate)
        {
            var summary = await _transactionService.GetFinancialSummaryAsync(startDate, endDate);
            return Ok(summary);
        }

        [HttpGet("monthly-summary")]
        public async Task<ActionResult<Dictionary<string, MonthlySummary>>> GetMonthlySummary(DateTime? startDate, DateTime? endDate)
        {
            var transactions = _transactionService.GetTransactions()
                .Where(t => (!startDate.HasValue || t.TransactionTime >= startDate) &&
                            (!endDate.HasValue || t.TransactionTime <= endDate)).ToList();

            var monthlyData = transactions
                .GroupBy(t => t.TransactionTime.ToString("yyyy-MM"))
                .ToDictionary(
                    g => g.Key,
                    g => new MonthlySummary
                    {
                        Income = g.Where(t => t.Category == "Nhận").Sum(t => t.Amount),
                        Expense = g.Where(t => t.Category == "Chi").Sum(t => t.Amount)
                    });

            return Ok(monthlyData);
        }
        //[HttpGet("fetch")]
        //public async Task<IActionResult> FetchTransactions()
        //{
        //    try
        //    {
        //        await _gmailService.ExtractTransactionsAsync();
        //        return Ok("Giao dịch đã được lưu vào transactions.xlsx");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Lỗi: {ex.Message}");
        //    }
        //}
    }
}