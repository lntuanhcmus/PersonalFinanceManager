using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Services;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.Shared.Dto;
using System.Globalization;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.API.Model;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.Shared.Data.Entity;
using AutoMapper;

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
        private readonly IMapper _mapper;


        public TransactionsApiController(GmailService gmailService,
                                         TransactionService transactionService,
                                         CategoryService categoryService,
                                         IOptions<GmailServiceSettings> gmailOptions,
                                         IMapper mapper)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _gmailSettings = gmailOptions.Value;
            _mapper = mapper;
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
            var result = _mapper.Map<TransactionDto>(transaction);
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

                var transaction = _mapper.Map<Transaction>(transactionDto);
                transaction.Status = transactionDto.TransactionTypeId == (int)TransactionTypeEnum.Advance ? (int)TransactionStatusEnum.Pending : (int)TransactionStatusEnum.Success;

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

            _mapper.Map(transactionDto, transaction);
            transaction.TransactionTime = DateTime.ParseExact(transactionDto.TransactionTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            await _transactionService.UpdateTransaction(transaction);

            var transactionCorrection = _mapper.Map<TransactionCorrection>(transaction);

            transactionCorrection.CreatedAt = DateTime.Now;
            await _transactionService.AddTransactionCorrection(transactionCorrection);
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

    }
}