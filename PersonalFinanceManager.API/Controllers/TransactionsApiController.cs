using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Services;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.Shared.Dto;
using System.Globalization;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.API.Data;

namespace PersonalFinanceManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsApiController : ControllerBase
    {
        private readonly GmailService _gmailService;
        private readonly TransactionService _transactionService;
        private readonly CategoryService _categoryService;


        public TransactionsApiController(GmailService gmailService, TransactionService transactionService, CategoryService categoryService)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
            _categoryService = categoryService;
        }

        //[HttpGet]
        //public ActionResult<List<Transaction>> Get()
        //{
        //    return Ok(_transactionService.GetTransactions());
        //}

        [HttpGet]
        public async Task<ActionResult<PagedResponse<TransactionDto>>> GetTransactions(
            string transactionId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            int? categoryId = null,
            int? transactionTypeId = null,
            string sourceAccount = null,
            string content = null,
            int? page = 1,
            int? pageSize = 10)
        {
            var result = await _transactionService.GetFilteredTransactionsAsync(
                transactionId, startDate, endDate, minAmount, maxAmount,
                categoryId, transactionTypeId, sourceAccount, content, page, pageSize);

            return Ok(result);
        }


        [HttpGet("get-by-id")]
        public async Task<ActionResult<TransactionDto>> GetById([FromQuery] string id)
        {
            var transaction = _transactionService.GetById(id);
            if (transaction == null)
            {
                return NotFound();
            }
            var result = new TransactionDto(transaction);
            return Ok(result);
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
            try
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
                    CategoryId = transactionDto.CategoryId
                };

                transaction = await ClassifyTransactionTypeByCategory(transactionDto.CategoryId, transaction);

                await _transactionService.AddTransaction(transaction);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(string id, [FromBody] TransactionDto transactionDto)
        {
            if (transactionDto == null || transactionDto.TransactionId != id)
                return BadRequest("Dữ liệu không hợp lệ");

            var transaction = _transactionService.GetById(id);
            if (transaction == null) return NotFound();

            // Cập nhật thông tin cơ bản
            transaction.TransactionTime = DateTime.ParseExact(transactionDto.TransactionTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            transaction.SourceAccount = transactionDto.SourceAccount;
            transaction.RecipientAccount = transactionDto.RecipientAccount;
            transaction.RecipientName = transactionDto.RecipientName;
            transaction.RecipientBank = transactionDto.RecipientBank;
            transaction.Amount = transactionDto.Amount;
            transaction.Description = transactionDto.Description;
            transaction.CategoryId = transactionDto.CategoryId;

            transaction = await ClassifyTransactionTypeByCategory(transactionDto.CategoryId, transaction);

            await _transactionService.UpdateTransaction(transaction);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var transaction = _transactionService.GetById(id);
            if (transaction == null)
                return NotFound();
            if (transaction.TransactionTypeId != (int)TransactionTypeEnum.Income)
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
                        Income = g.Where(t => t.TransactionTypeId == (int)TransactionTypeEnum.Income).Sum(t => t.Amount),
                        Expense = g.Where(t => t.TransactionTypeId == (int)TransactionTypeEnum.Expense).Sum(t => t.Amount)
                    });

            return Ok(monthlyData);
        }

        [HttpGet("get-transaction-types")]
        public async Task<ActionResult<List<TransactionType>>> GetTransactionTypes()
        {
            var transactionTypes = _transactionService.GetTransactionTypes();

            return Ok(transactionTypes);
        }


        private async Task<Transaction> ClassifyTransactionTypeByCategory(int? categoryId, Transaction transaction)
        {
            if (categoryId.HasValue)
            {
                var category = await _categoryService.GetByIdAsync(categoryId.Value);
                if (category == null)
                    return null;

                transaction.TransactionTypeId = category.TransactionTypeId;
            }

            return transaction;
        }
    }
}