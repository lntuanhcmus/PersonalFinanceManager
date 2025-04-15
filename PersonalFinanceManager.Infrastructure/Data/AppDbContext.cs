using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Data;

namespace PersonalFinanceManager.Infrastructure.Data
{
    public class AppDbContext: IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }


        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<TransactionType> TransactionTypes { get; set; }

        public DbSet<Budget> Budgets { get; set; }

        public DbSet<LabelingRule> LabelingRules { get; set; }

        public DbSet<RepaymentTransaction> RepaymentTransactions { get; set; }

        public DbSet<TransactionCorrection> TransactionCorrections { get; set; }

        public DbSet<ExternalToken> ExternalTokens { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RepaymentTransaction>()
            .HasOne(rt => rt.Transaction)
            .WithMany(t => t.RepaymentTransactions)
            .HasForeignKey(rt => rt.TransactionId)
            .HasPrincipalKey(t => t.TransactionId);

            modelBuilder.Entity<TransactionCorrection>()
            .HasKey(t => t.Id);

            modelBuilder.Entity<TransactionCorrection>()
                .Property(t => t.TransactionId)
                .IsRequired();

            modelBuilder.Entity<TransactionCorrection>()
                .Property(t => t.Description)
                .IsRequired();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId);
            // Thêm ràng buộc khác nếu cần

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId);

            modelBuilder.Entity<TransactionType>().HasData(
                new TransactionType { Id = 1, Name = "Thu Nhập", Code = "Income" },
                new TransactionType { Id = 2, Name = "Chi Trả", Code = "Expense"},
                new TransactionType { Id = 3, Name = "Tạm Ứng", Code = "Advance" },
                new TransactionType { Id = 4, Name = "Hoàn Trả", Code = "Repayment" }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Lương", Code = "LU", TransactionTypeId = 1 },
                new Category { Id = 2, Name = "Thưởng", Code = "TH", TransactionTypeId = 1 },
                new Category { Id = 3, Name = "Sinh Hoạt", Code = "SH", TransactionTypeId = 2 },
                new Category { Id = 4, Name = "Học Tập và Phát Triển", Code = "HT-PT", TransactionTypeId = 2 },
                new Category { Id = 5, Name = "Giải trí và Phát Sinh", Code ="GT-PS", TransactionTypeId = 2 }
            );
            modelBuilder.Entity<LabelingRule>().HasData(
                new LabelingRule { Id = 1, Keyword = "KOVQR", TransactionTypeId = 2, CategoryId = 3 },
                new LabelingRule { Id = 2, Keyword = "LE NGUYEN TUAN chuyen khoan", TransactionTypeId = 2, CategoryId = 5 },
                new LabelingRule { Id = 3, Keyword = "603 - 60.7 UVK", TransactionTypeId = 3, CategoryId = 3 },
                new LabelingRule { Id = 4, Keyword = "tien cau long", TransactionTypeId = 2, CategoryId = 4 },
                new LabelingRule { Id = 5, Keyword = "Tien Banh Mi", TransactionTypeId = 2, CategoryId = 3 },
                new LabelingRule { Id = 6, Keyword = "Tien com toi", TransactionTypeId = 2, CategoryId = 3 },
                new LabelingRule { Id = 7, Keyword = "Tien com trua", TransactionTypeId = 2, CategoryId = 3 },
                new LabelingRule { Id = 8, Keyword = "GT-PS", TransactionTypeId = 2, CategoryId = 5 },
                new LabelingRule { Id = 9, Keyword = "HT-PT", TransactionTypeId = 2, CategoryId = 4 },
                new LabelingRule { Id = 10, Keyword = "SH - ", TransactionTypeId = 2, CategoryId = 3 },
                new LabelingRule { Id = 11, Keyword = "Lương tháng", TransactionTypeId = 1, CategoryId = 1 }
            );
        }
    }
}
