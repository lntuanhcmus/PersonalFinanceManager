using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Models;

namespace PersonalFinanceManager.API.Data
{
    public class TransactionContext: DbContext
    {
        public TransactionContext(DbContextOptions<TransactionContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
