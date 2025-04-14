using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.API.Helper;
using PersonalFinanceManager.Shared.Constants;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Models;
using System.Globalization;
using System.Text;
using PersonalFinanceManager.Shared.Data.Entity;

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
            int? status,
            int? page,
            int? pageSize)
        {
            var query = ApplyTransactionFilters(transactionId, startDate, endDate, minAmount, maxAmount,
                                                categoryId, transactionTypeId, sourceAccount, content, status);

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
                    TransactionTypeName = x.TransactionType != null ? x.TransactionType.Name : null,
                    Status = x.Status,
                    RepaymentAmount = x.RepaymentAmount,
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

        public async Task<byte[]> ExportFilteredTransactionsAsCsvAsync(
            string transactionId,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minAmount,
            decimal? maxAmount,
            int? categoryId,
            int? transactionTypeId,
            string sourceAccount,
            string content,
            int? status)
        {
            var query = ApplyTransactionFilters(transactionId, startDate, endDate, minAmount, maxAmount,
                                                categoryId, transactionTypeId, sourceAccount, content, status)
                                                .OrderByDescending(t => t.TransactionTime);

            var transactions = await query.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("TransactionId,TransactionTime,Amount,ActualAmount,SourceAccount,Description,Category,Type,Status");

            foreach (var t in transactions)
            {
                // Format tiền với dấu chấm, loại bỏ phần thập phân nếu không cần
                string amountStr = t.Amount.ToString("0.##", CultureInfo.InvariantCulture);
                string actualAmountStr = (t.Amount - t.RepaymentAmount).ToString("0.##", CultureInfo.InvariantCulture);

                // Escape dấu phẩy trong mô tả nếu có
                string description = t.Description?.Replace(",", " ") ?? "";
                string category = t.Category?.Name?.Replace(",", " ") ?? "";
                string type = t.TransactionType?.Name?.Replace(",", " ") ?? "";

                sb.AppendLine($"{t.TransactionId}," +
                              $"{t.TransactionTime:dd/MM/yyyy HH:mm}," +
                              $"{amountStr}," +
                              $"{actualAmountStr}," +
                              $"{t.SourceAccount}," +
                              $"{description}," +
                              $"{category}," +
                              $"{type}," +
                              $"{t.Status}");
            }

            // UTF-8 with BOM để Excel hiểu đúng encoding tiếng Việt
            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        private IQueryable<Transaction> ApplyTransactionFilters(
            string transactionId,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minAmount,
            decimal? maxAmount,
            int? categoryId,
            int? transactionTypeId,
            string sourceAccount,
            string content,
            int? status)
        {
            var query = _context.Transactions
                .AsNoTracking()
                .Include(t => t.TransactionType)
                .Include(t => t.Category)
                .AsQueryable();

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

            if (status.HasValue)
                query = query.Where(t => t.Status == status);

            return query;
        }

        public Transaction GetById(string id)
        {
            return _context.Transactions.Include(x=>x.TransactionType).Include(x=>x.Category).FirstOrDefault(x=>x.TransactionId == id);
        }

        public async Task<FinancialSummary> GetFinancialSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            var transactions = await _context.Transactions
                .Include(t => t.RepaymentTransactions) // để tránh truy vấn lại trong vòng lặp
                .Where(t => (!startDate.HasValue || t.TransactionTime >= startDate) &&
                            (!endDate.HasValue || t.TransactionTime <= endDate))
                .ToListAsync();

            var totalIncome = transactions
                .Where(t => t.TransactionTypeId == (int)TransactionTypeEnum.Income)
                .Sum(t => t.Amount);

            var totalAdvance = transactions
                .Where(t => t.TransactionTypeId == (int)TransactionTypeEnum.Advance)
                .Sum(t =>
                {
                    if (t.Status == (int)TransactionStatusEnum.Success)
                    {
                        var repayment = t.RepaymentTransactions?.Sum(r => r.Amount) ?? 0;
                        return t.Amount - repayment;
                    }
                    return t.Amount;
                });

            var totalExpense = TransactionHelper.CalculateActualExpense(
                transactions,
                (int)TransactionTypeEnum.Expense,
                (int)TransactionTypeEnum.Advance,
                (int)TransactionStatusEnum.Success
            );

            var balance = totalIncome - totalExpense;

            var categoryBreakdown = transactions
                .GroupBy(t => t.TransactionTypeId)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            return new FinancialSummary
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                TotalAdvance = totalAdvance,
                Balance = balance,
                CategoryBreakdown = categoryBreakdown
            };
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

            var defaultCategory = categories.FirstOrDefault(c => c.Code == CategoryCodes.SINH_HOAT); // "GT-PS"
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
                tx.Status = tx.TransactionTypeId == (int)TransactionTypeEnum.Advance ? tx.Status = (int)TransactionStatusEnum.Pending : tx.Status = (int)TransactionStatusEnum.Success;
            }

            return transactions;
            
        }

        public List<TransactionType> GetTransactionTypes()
        {
            return _context.TransactionTypes.ToList();
        }


    }
}
