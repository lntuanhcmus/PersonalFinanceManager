using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Dto;

namespace PersonalFinanceManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RepaymentTransactionApiController : ControllerBase
    {
        private readonly RepaymentTransactionService _repaymentTransactionService;
        private readonly IMapper _mapper;

        public RepaymentTransactionApiController(
            RepaymentTransactionService repaymentTransactionService,
            IMapper mapper)
        {
            _repaymentTransactionService = repaymentTransactionService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<RepaymentTransactionDto>> GetAll(string transactionId)
        {
            var transactionList = await _repaymentTransactionService.GetByTransactionId(transactionId);
            var result = _mapper.Map<IEnumerable<RepaymentTransactionDto>>(transactionList);

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
            var result = _mapper.Map<RepaymentTransactionDto>(transaction);
            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] RepaymentTransactionDto transactionDto)
        {
            try
            {
                if (transactionDto == null || string.IsNullOrEmpty(transactionDto.TransactionId))
                    return BadRequest("Dữ liệu không hợp lệ");

                var transaction = _mapper.Map<RepaymentTransaction>(transactionDto);

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
