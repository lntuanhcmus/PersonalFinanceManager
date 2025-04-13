using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Services;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.Shared.Dto;
using System.Globalization;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.API.Model;
using Microsoft.Extensions.Options;

namespace PersonalFinanceManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsApiController : ControllerBase
    {
        private readonly GmailService _gmailService;
        private readonly TransactionService _transactionService;
        private readonly CategoryService _categoryService;
        private readonly GmailServiceSettings _gmailSettings;


        public TransactionsApiController(GmailService gmailService,
                                         TransactionService transactionService,
                                         CategoryService categoryService,
                                         IOptions<GmailServiceSettings> gmailOptions)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _gmailSettings = gmailOptions.Value;
        }

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
            int? status = null,
            int? page = 1,
            int? pageSize = 10)
        {
            var result = await _transactionService.GetFilteredTransactionsAsync(
                transactionId, startDate, endDate, minAmount, maxAmount,
                categoryId, transactionTypeId, sourceAccount, content, status, page, pageSize);

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
            int maxResult = _gmailSettings.MaxResult;

            // Kiểm tra hợp lệ, nếu không dùng mặc định
            if (maxResult <= 0) maxResult = 10;

            var transactions = await _gmailService.ExtractTransactionsAsync("credentials.json", "token.json", maxResult);
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
                    CategoryId = transactionDto.CategoryId,
                    TransactionTypeId = transactionDto.TransactionTypeId,
                    Status = transactionDto.TransactionTypeId == (int)TransactionTypeEnum.Advance ? (int)TransactionStatusEnum.Pending : (int)TransactionStatusEnum.Success,
                };

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
            transaction.TransactionTypeId = transactionDto.TransactionTypeId;
            transaction.Status = transactionDto.Status;

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

        [HttpGet("get-transaction-types")]
        public async Task<ActionResult<List<TransactionType>>> GetTransactionTypes()
        {
            var transactionTypes = _transactionService.GetTransactionTypes();

            return Ok(transactionTypes);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportTransactions(
            string transactionId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            int? categoryId = null,
            int? transactionTypeId = null,
            string sourceAccount = null,
            string content = null,
            int? status = null)
        {
            var fileBytes = await _transactionService.ExportFilteredTransactionsAsCsvAsync(
                transactionId, startDate, endDate, minAmount, maxAmount,
                categoryId, transactionTypeId, sourceAccount, content, status);

            var fileName = $"transactions_{DateTime.Now:yyyyMMddHHmmss}.csv";
            return File(fileBytes, "text/csv", fileName);
        }

        //private async Task<Transaction> ClassifyTransactionTypeByCategory(int? categoryId, Transaction transaction)
        //{
        //    if (categoryId.HasValue)
        //    {
        //        var category = await _categoryService.GetByIdAsync(categoryId.Value);
        //        if (category == null)
        //            return null;

        //        transaction.TransactionTypeId = category.TransactionTypeId;
        //    }

        //    return transaction;
        //}



        //Statistic Api




    }
}