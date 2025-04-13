using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Models;
using System.Globalization;
using static System.TimeZoneInfo;

namespace PersonalFinanceManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepaymentTransactionApiController : ControllerBase
    {
        private readonly RepaymentTransactionService _repaymentTransactionService;

        public RepaymentTransactionApiController(RepaymentTransactionService repaymentTransactionService)
        {
            _repaymentTransactionService = repaymentTransactionService;
        }

        [HttpGet]
        public async Task<ActionResult<RepaymentTransactionDto>> GetAll(string transactionId)
        {
            var transactionList = await _repaymentTransactionService.GetByTransactionId(transactionId);
            var result = transactionList.Select(x => new RepaymentTransactionDto(x));

            return Ok(result);
        }


        [HttpGet("get-by-id")]
        public async Task<ActionResult<RepaymentTransactionDto>> GetById([FromQuery] int id)
        {
            var transaction = _repaymentTransactionService.GetById(id);
            if (transaction == null)
            {
                return NotFound();
            }
            var result = new RepaymentTransactionDto(transaction);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RepaymentTransactionDto transactionDto)
        {
            try
            {
                if (transactionDto == null || string.IsNullOrEmpty(transactionDto.TransactionId))
                    return BadRequest("Dữ liệu không hợp lệ");


                //var transaction = transactionDto.RevertTransactionFromDto();

                var transaction = new RepaymentTransaction()
                {
                    Amount = transactionDto.Amount,
                    CreatedAt = DateTime.Now,
                    TransactionTime = DateTime.ParseExact(transactionDto.TransactionTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    Description = transactionDto.Description,
                    SenderName = transactionDto.SenderName,
                    TransactionId = transactionDto.TransactionId
                };

                await _repaymentTransactionService.AddTransaction(transaction);
                return Ok(new { message = "Giao dịch hoàn trả được tạo thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { error = "Lỗi server, vui lòng thử lại" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] RepaymentTransactionDto transactionDto)
        {
            try
            {
                if (transactionDto == null || transactionDto.Id != id)
                    return BadRequest("Dữ liệu không hợp lệ");

                var transaction = _repaymentTransactionService.GetById(id);
                if (transaction == null) return NotFound();

                // Cập nhật thông tin cơ bản
                transaction.Amount = transactionDto.Amount;
                transaction.Description = transactionDto.Description;
                transaction.SenderName = transactionDto.SenderName;

                await _repaymentTransactionService.UpdateTransaction(transaction);
                return Ok(new { message = "Giao dịch hoàn trả được cập nhật thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { error = "Lỗi server, vui lòng thử lại" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var transaction = _repaymentTransactionService.GetById(id);
                if (transaction == null)
                    return NotFound();

                await _repaymentTransactionService.DeleteTransaction(id); // Cần thêm hàm này
                return Ok(new { message = "Xóa giao dịch hoàn trả thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { error = "Lỗi server, vui lòng thử lại" });
            }
        }
    }
}
