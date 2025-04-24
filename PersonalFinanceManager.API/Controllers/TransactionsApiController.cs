using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.Shared.Dto;
using System.Globalization;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.API.Model;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.Shared.Data;
using AutoMapper;
using PersonalFinanceManager.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using System.Text;

namespace PersonalFinanceManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsApiController : ControllerBase
    {
        private readonly IGmailService _gmailService;
        private readonly TransactionService _transactionService;
        private readonly CategoryService _categoryService;
        private readonly GmailServiceSettings _gmailSettings;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransactionsApiController> _logger;
        private readonly IQueueService _queueService;


        public TransactionsApiController(IGmailService gmailService,
                                         TransactionService transactionService,
                                         CategoryService categoryService,
                                         IOptions<GmailServiceSettings> gmailOptions,
                                         IMapper mapper,
                                         ILogger<TransactionsApiController> logger,
                                         IConfiguration configuration,
                                         IQueueService queueService)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _gmailSettings = gmailOptions.Value;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _queueService = queueService;
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
            bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if(!isValid)
            {
                return BadRequest("UserId không hợp lệ");
            }

            var result = await _transactionService.GetFilteredTransactionsAsync(
            userId,
            transactionId, startDate, endDate, minAmount, maxAmount,
            categoryId, transactionTypeId, sourceAccount, content, status, page, pageSize);

            return Ok(result);
            

        }


        [HttpGet("get-by-id")]
        public async Task<ActionResult<TransactionDto>> GetById([FromQuery] string id)
        {
            bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (!isValid)
            {
                return BadRequest("UserId không hợp lệ");
            }

            var transaction = _transactionService.GetById(id, userId);
            if (transaction == null)
            {
                return NotFound();
            }
            var result = _mapper.Map<TransactionDto>(transaction);
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] TransactionDto transactionDto)
        {
            try
            {
                bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
                if (!isValid)
                {
                    return BadRequest("UserId không hợp lệ");
                }
                if (transactionDto == null || string.IsNullOrEmpty(transactionDto.TransactionId))
                    return BadRequest("Dữ liệu không hợp lệ");

                var transaction = _mapper.Map<Transaction>(transactionDto);
                transaction.Status = transactionDto.TransactionTypeId == (int)TransactionTypeEnum.Advance ? (int)TransactionStatusEnum.Pending : (int)TransactionStatusEnum.Success;
                transaction.UserId = userId;
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
            bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (!isValid)
            {
                return BadRequest("User is invalid");
            }
            var transaction = _transactionService.GetById(id, userId);
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
            if (Int16.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out short userId))
            {
                var fileBytes = await _transactionService.ExportFilteredTransactionsAsCsvAsync(userId,
                transactionId, startDate, endDate, minAmount, maxAmount,
                categoryId, transactionTypeId, sourceAccount, content, status);

                var fileName = $"transactions_{DateTime.Now:yyyyMMddHHmmss}.csv";
                return File(fileBytes, "text/csv", fileName);
            }
            else
            {
                return BadRequest("userId is invalid");
            }
            
        }

        [HttpGet("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userCredential = await _gmailService.GetCredentialFromToken(userId, "credentials.json");

                var transactions = await _gmailService.ExtractTransactionsAsync(userId, userCredential, 10);

                if(transactions == null || !transactions.Any())
                {
                    _logger.LogInformation("No transactions found for user {UserId}", userId);
                    return Ok("No new transactios found");
                }

                bool queueEnabled = _configuration.GetValue<bool>("AzureQueue:Enabled", false);
                if(queueEnabled)
                {
                    foreach (var transaction in transactions)
                    {
                        try
                        {
                            var json = JsonConvert.SerializeObject(transaction); // Bước 1: convert thành JSON string
                            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)); // Bước 2: encode base64
                            await _queueService.SendMessageAsync(base64);
                            _logger.LogInformation("Queued transaction for user {UserId}: {TransactionId}", userId, transaction.TransactionId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to queue transaction {TransactionId} for user {UserId}", transaction.TransactionId, userId);
                        }
                    }
                }
                else
                {
                    await _transactionService.SaveTransactions(transactions, int.Parse(userId));
                    _logger.LogInformation("Saved {TransactionCount} transactions directly for user {UserId}", transactions.Count, userId);
                }

                return Ok("Transactions processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing transaction for user");
                return BadRequest("Failed to process transactions");
            }
        }
    }
}