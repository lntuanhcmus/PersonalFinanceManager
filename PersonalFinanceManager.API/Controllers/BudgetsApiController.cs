using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Infrastructure.Data;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Models;
using System.Globalization;
using PersonalFinanceManager.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BudgetsApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly BudgetService _budgetService;
    private readonly IMapper _mapper;

    public BudgetsApiController(AppDbContext context, BudgetService budgetService, IMapper mapper)
    {
        _context = context;
        _budgetService = budgetService;
        _mapper = mapper;
    }

    // GET: api/budgets
    [HttpGet]
    public async Task<ActionResult<PagedResponse<BudgetDto>>> GetBudgets(
            string transactionId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            int? categoryId = null,
            string period = null,
            int? page = 1,
            int? pageSize = 10)
    {
        bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
        if (!isValid)
        {
            return BadRequest("UserId không hợp lệ");
        }
        var result = await _budgetService.GetFilteredBudgetsAsync(userId,
            categoryId, startDate, endDate, minAmount, maxAmount, period, page, pageSize);

        return Ok(result);
    }

    // GET: api/budgets/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Budget>> GetBudget(int id)
    {
        var budget = await _context.Budgets.FindAsync(id);
        if (budget == null) return NotFound();
        return Ok(budget);
    }

    // POST: api/budgets
    [HttpPost]
    public async Task<ActionResult<Budget>> CreateBudget(BudgetDto budgetDto)
    {
        try
        {
            bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (!isValid)
            {
                return BadRequest("UserId không hợp lệ");
            }

            if (budgetDto == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var budget = new Budget()
            {
                Amount = budgetDto.Amount,
                StartDate = DateTime.ParseExact(budgetDto.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                EndDate = !String.IsNullOrEmpty(budgetDto.EndDate) ? DateTime.ParseExact(budgetDto.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1).AddTicks(-1): null,
                CategoryId = budgetDto.CategoryId,
                Period = budgetDto.Period,
                UserId = userId
            };


            await _budgetService.AddBudget(budget);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest();
            throw;
        } 
    }

    // PUT: api/budgets/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBudget(int id, BudgetDto budgetDto)
    {
        try
        {
            if (budgetDto == null || budgetDto.Id != id)
                return BadRequest("Dữ liệu không hợp lệ");

            bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (!isValid)
            {
                return BadRequest("UserId không hợp lệ");
            }

            var budget = _budgetService.GetById(id, userId);
            if (budget == null) return NotFound();

            // Cập nhật thông tin cơ bản
            budget.StartDate = DateTime.ParseExact(budgetDto.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            budget.EndDate = budgetDto.EndDate != null ? DateTime.ParseExact(budgetDto.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1).AddTicks(-1) : null;
            budget.Period = budgetDto.Period;
            budget.Amount = budgetDto.Amount;
            budget.CategoryId = budgetDto.CategoryId;

            await _budgetService.UpdateBudget(budget);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/budgets/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
        if (!isValid)
        {
            return BadRequest("UserId không hợp lệ");
        }
        var budget = _budgetService.GetById(id, userId);
        if (budget == null)
            return NotFound();

        await _budgetService.DeleteBudget(id); // Cần thêm hàm này
        return Ok();
    }

    [HttpGet("get-by-id")]
    public async Task<ActionResult<BudgetDto>> GetById([FromQuery] int id)
    {
        bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
        if (!isValid)
        {
            return BadRequest("UserId không hợp lệ");
        }

        var budget = _budgetService.GetById(id, userId);
        if (budget == null)
        {
            return NotFound();
        }
        var result = _mapper.Map<BudgetDto>(budget);
        return Ok(result);
    }
}