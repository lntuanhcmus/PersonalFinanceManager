using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Models;

namespace PersonalFinanceManager.API.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<TransactionType> TransactionTypes { get; set; }

        public DbSet<Budget> Budgets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransactionType>().HasData(
                new TransactionType { Id = 1, Name = "Thu Nhập", Code = "Income" },
                new TransactionType { Id = 2, Name = "Chi Trả", Code = "Expense"}
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Lương", Code = "LU", TransactionTypeId = 1 },
                new Category { Id = 2, Name = "Thưởng", Code = "TH", TransactionTypeId = 1 },
                new Category { Id = 3, Name = "Sinh Hoạt", Code = "SH", TransactionTypeId = 2 },
                new Category { Id = 4, Name = "Học Tập và Phát Triển", Code = "HT-PT", TransactionTypeId = 2 },
                new Category { Id = 5, Name = "Giải trí và Phát Sinh", Code ="GT-PS", TransactionTypeId = 2 }
            );
        }
    }
}
