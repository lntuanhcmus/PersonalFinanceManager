using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.API.Data;
using PersonalFinanceManager.Shared.Constants;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Models;

namespace PersonalFinanceManager.API.Services
{
    public class TransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context) 
        {
            _context = context;
        }

        public List<Transaction> GetTransactions()
        {
            return _context.Transactions
                .OrderByDescending(t => t.TransactionTime)
                .ToList();
        }

        public async Task<PagedResponse<TransactionDto>> GetFilteredTransactionsAsync(
            string transactionId,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minAmount,
            decimal? maxAmount,
            int? categoryId,
            int? transactionTypeId,
            string sourceAccount,
            string content,
            int? page,
            int? pageSize)
        {
            var query = _context.Transactions
                .AsNoTracking()
                .Include(t => t.TransactionType)
                .Include(t => t.Category)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(transactionId))
                query = query.Where(t => t.TransactionId.Contains(transactionId));

            if (startDate.HasValue)
                query = query.Where(t => t.TransactionTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.TransactionTime <= endDate.Value);

            if (minAmount.HasValue)
                query = query.Where(t => t.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(t => t.Amount <= maxAmount.Value);

            if (!string.IsNullOrEmpty(sourceAccount))
                query = query.Where(t => t.SourceAccount.Contains(sourceAccount));

            if (!string.IsNullOrEmpty(content))
                query = query.Where(t => t.Description.Contains(content));

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId);

            if (transactionTypeId.HasValue)
                query = query.Where(t => t.TransactionTypeId == transactionTypeId);

            // Lấy tổng số phần tử trước phân trang
            var totalItems = await query.CountAsync();

            query = query
               .OrderByDescending(t => t.TransactionTime);

            if (page != null && pageSize != null)
            {
                query = query
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
            }

            // Phân trang và chuyển sang DTO
            var pagedItems = await query
                .Select(x => new TransactionDto
                {
                    Amount = x.Amount,
                    CategoryId = x.CategoryId,
                    SourceAccount = x.SourceAccount,
                    Description = x.Description,
                    RecipientAccount = x.RecipientAccount,
                    RecipientBank = x.RecipientBank,
                    RecipientName = x.RecipientName,
                    TransactionId = x.TransactionId,
                    TransactionTime = x.TransactionTime.ToString("dd/MM/yyyy HH:mm"),
                    TransactionTypeId = x.TransactionTypeId,
                    CategoryName = x.Category != null ? x.Category.Name : null,
                    TransactionTypeName = x.TransactionType != null ? x.TransactionType.Name : null
                })
                .ToListAsync();

            // Trả kết quả dạng phân trang
            return new PagedResponse<TransactionDto>
            {
                Items = pagedItems,
                TotalItems = totalItems,
                PageNumber = page ?? 0,
                PageSize = pageSize ?? 0
            };
        }

        public Transaction GetById(string id)
        {
            return _context.Transactions.Include(x=>x.TransactionType).Include(x=>x.Category).FirstOrDefault(x=>x.TransactionId == id);
        }

        public async Task<FinancialSummary> GetFinancialSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => (!startDate.HasValue || t.TransactionTime >= startDate) &&
                            (!endDate.HasValue || t.TransactionTime <= endDate))
                .ToListAsync();

            var summary = new FinancialSummary
            {
                TotalIncome = transactions.Where(t => t.TransactionTypeId == (int)TransactionTypeEnum.Income).Sum(t => t.Amount),
                TotalExpense = transactions.Where(t => t.TransactionTypeId == (int)TransactionTypeEnum.Expense).Sum(t => t.Amount),
                Balance = transactions.Sum(t => t.TransactionTypeId == (int)TransactionTypeEnum.Income ? t.Amount : -t.Amount),
                CategoryBreakdown = transactions
                    .GroupBy(t => t.TransactionTypeId)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount))
            };

            return summary;
        }

        public async Task AddTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTransaction(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTransaction(string id)
        {
            var transaction = _context.Transactions.Find(id);
            if (transaction != null && transaction.TransactionTypeId == (int)TransactionTypeEnum.Income)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveTransactions(List<Transaction> transactions)
        {
            var existingIds = _context.Transactions.Select(t => t.TransactionId).ToList();
            var newTransactions = transactions.Where(t => !existingIds.Contains(t.TransactionId)).ToList();
            if(newTransactions.Any())
            {
                newTransactions = await ClassifyTransactionsAsync(newTransactions);
            }
            _context.Transactions.AddRange(newTransactions);
            await _context.SaveChangesAsync();
        }

        private async Task<List<Transaction>> ClassifyTransactionsAsync(List<Transaction> transactions)
        {

            // Lấy toàn bộ LabelingRules từ DB
            var labelingRules = await _context.LabelingRules.ToListAsync();

            var categories = await _context.Categories.ToListAsync();

            var defaultCategory = categories.FirstOrDefault(c => c.Code == CategoryCodes.GIAI_TRI); // "GT-PS"
            var defaultTransactionType = _context.TransactionTypes.FirstOrDefault(t => t.Code == TransactionTypeConstant.Expense);

            foreach (var tx in transactions)
            {
                if (string.IsNullOrWhiteSpace(tx.Description))
                    continue;

                // Tìm rule đầu tiên khớp keyword trong mô tả (case-insensitive)
                var matchedRule = labelingRules
                    .FirstOrDefault(rule => tx.Description.Contains(rule.Keyword, StringComparison.OrdinalIgnoreCase));

                if (matchedRule != null)
                {
                    tx.CategoryId = matchedRule.CategoryId;
                    tx.TransactionTypeId = matchedRule.TransactionTypeId;
                }
                else
                {
                    tx.TransactionTypeId = defaultTransactionType.Id;
                    tx.CategoryId = defaultCategory.Id;
                }
            }

            return transactions;
            
        }

        public List<TransactionType> GetTransactionTypes()
        {
            return _context.TransactionTypes.ToList();
        }


    }
}
