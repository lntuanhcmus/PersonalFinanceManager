using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Models;

namespace PersonalFinanceManager.API.Services
{
    public class BudgetService
    {
        private readonly AppDbContext _context;
        public BudgetService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResponse<BudgetDto>> GetFilteredBudgetsAsync(
            int? categoryId,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minAmount,
            decimal? maxAmount,
            string period,
            int? page,
            int? pageSize)
        {
            var query = _context.Budgets
                .AsNoTracking()
                .Include(t => t.Category)
                .AsQueryable();

            // Apply filters

            if (startDate.HasValue)
            {
                query = query.Where(t =>
                    t.StartDate >= startDate.Value &&
                    (!t.EndDate.HasValue || t.EndDate >= startDate.Value)); // đảm bảo khoảng thời gian có thể mở
            }

            if (endDate.HasValue)
            {
                query = query.Where(t =>
                    (!t.EndDate.HasValue || t.EndDate <= endDate.Value) &&
                    t.StartDate <= endDate.Value); // đảm bảo không vượt qua thời gian kết thúc
            }

            if (minAmount.HasValue)
                query = query.Where(t => t.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(t => t.Amount <= maxAmount.Value);

            if (!string.IsNullOrEmpty(period))
                query = query.Where(t => t.Period == period);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId);

            // Lấy tổng số phần tử trước phân trang
            var totalItems = await query.CountAsync();

            if (page != null && pageSize != null)
            {
                query = query
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
            }

            // Phân trang và chuyển sang DTO
            var pagedItems = await query
                .Select(x => new BudgetDto
                {
                    Amount = x.Amount,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null ? x.Category.Name : null,
                    Id = x.Id,
                    Period = x.Period,
                    StartDate = x.StartDate.ToString("dd/MM/yyyy HH:mm"),
                    EndDate = x.EndDate.HasValue ? x.EndDate.Value.ToString("dd/MM/yyyy HH:mm") : null
                })
                .ToListAsync();

            // Trả kết quả dạng phân trang
            return new PagedResponse<BudgetDto>
            {
                Items = pagedItems,
                TotalItems = totalItems,
                PageNumber = page ?? 0,
                PageSize = pageSize ?? 0
            };
        }

        public async Task AddBudget(Budget budget)
        {
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBudget(Budget budget)
        {
            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBudget(int id)
        {
            var budget = _context.Budgets.Find(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
        }

        public Budget GetById(int id)
        {
            return _context.Budgets.Include(x => x.Category).FirstOrDefault(x => x.Id == id);
        }
    }
}
