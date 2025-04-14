using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Data.Entity;

namespace PersonalFinanceManager.API.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;
        public CategoryService(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id); 
        }
    }
}
