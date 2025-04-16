using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Infrastructure.Data;
using PersonalFinanceManager.API.Helper;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PersonalFinanceManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsApiController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TransactionService _transactionService;
        public StatisticsApiController(AppDbContext context, TransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        [HttpGet("budget-usage")]
        public async Task<IActionResult> GetBudgetUsage()
        {
            bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (!isValid)
            {
                return BadRequest("UserId không hợp lệ");
            }

            var budgets = _context.Budgets
                .Include(b => b.Category)
                .Where(x=>x.UserId == userId)
                .ToList();

            var transactions = _context.Transactions
                .Include(t => t.TransactionType)
                .Include(t => t.RepaymentTransactions)
                .Where(x => x.UserId == userId)
                .ToList();

            var expenseTypeId = _context.TransactionTypes
                .FirstOrDefault(t => t.Code == "Expense")?.Id;

            var advanceTypeId = _context.TransactionTypes
                .FirstOrDefault(t => t.Code == "Advance")?.Id;

            var successStatus = (int)TransactionStatusEnum.Success;

            var results = budgets.Select(b =>
            {
                var spent = TransactionHelper.CalculateActualExpense(
                    transactions,
                    expenseTypeId ?? 0,
                    advanceTypeId ?? 0,
                    successStatus,
                    b.CategoryId,
                    b.StartDate,
                    b.EndDate
                );

                return new BudgetUsage
                {
                    CategoryName = b.Category?.Name,
                    BudgetAmount = b.Amount,
                    SpentAmount = spent
                };
            }).ToList();

            return Ok(results);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<FinancialSummary>> GetFinancialSummary(DateTime? startDate, DateTime? endDate)
        {
            bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (!isValid)
            {
                return BadRequest("UserId không hợp lệ");
            }
            var summary = await _transactionService.GetFinancialSummaryAsync(startDate, endDate, userId);
            return Ok(summary);
        }

        [HttpGet("monthly-summary")]
        public async Task<ActionResult<Dictionary<string, MonthlySummary>>> GetMonthlySummary(DateTime? startDate, DateTime? endDate)
        {
            bool isValid = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            if (!isValid)
            {
                return BadRequest("UserId không hợp lệ");
            }

            var transactions = _transactionService.GetTransactions()
                .Where(t => t.UserId == userId && (!startDate.HasValue || t.TransactionTime >= startDate) &&
                            (!endDate.HasValue || t.TransactionTime <= endDate))
                .ToList();

            var monthlyData = transactions
                .GroupBy(t => t.TransactionTime.ToString("yyyy-MM"))
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var income = g
                            .Where(t => t.TransactionTypeId == (int)TransactionTypeEnum.Income)
                            .Sum(t => t.Amount);

                        var expense = TransactionHelper.CalculateActualExpense(
                            g.ToList(),
                            (int)TransactionTypeEnum.Expense,
                            (int)TransactionTypeEnum.Advance,
                            (int)TransactionStatusEnum.Success
                        );

                        return new MonthlySummary
                        {
                            Income = income,
                            Expense = expense
                        };
                    });

            return Ok(monthlyData);
        }
    }
}
