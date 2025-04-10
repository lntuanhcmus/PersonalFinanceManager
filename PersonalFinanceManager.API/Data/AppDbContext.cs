using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Models;

namespace PersonalFinanceManager.API.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Budget> Budgets { get; set; }
    }
}
