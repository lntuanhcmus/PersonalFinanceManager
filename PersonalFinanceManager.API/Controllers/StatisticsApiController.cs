using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.API.Data;

namespace PersonalFinanceManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsApiController : Controller
    {
        private readonly AppDbContext _context;
        public StatisticsApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("budget-usage")]
        public IActionResult GetBudgetUsage()
        {
            var budgets = _context.Budgets
                .Include(b => b.Category)
                .ToList();

            var transactions = _context.Transactions
                .Include(t => t.TransactionType)
                .ToList();

            var expenseTypeId = _context.TransactionTypes
                .FirstOrDefault(t => t.Code == "Expense")?.Id;

            var results = budgets.Select(b =>
            {
                var spent = transactions
                    .Where(t => t.TransactionTypeId == expenseTypeId
                                && t.CategoryId == b.CategoryId
                                && t.TransactionTime >= b.StartDate
                                && t.TransactionTime <= b.EndDate)
                    .Sum(t => t.Amount);

                return new
                {
                    CategoryName = b.Category?.Name,
                    BudgetAmount = b.Amount,
                    SpentAmount = spent
                };
            }).ToList();

            return Ok(results);
        }
    }
}
