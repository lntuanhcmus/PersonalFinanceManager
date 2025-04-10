using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.API.Data;
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

        public Transaction GetById(string id)
        {
            return _context.Transactions.Find(id);
        }

        public async Task<FinancialSummary> GetFinancialSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => (!startDate.HasValue || t.TransactionTime >= startDate) &&
                            (!endDate.HasValue || t.TransactionTime <= endDate))
                .ToListAsync();

            var summary = new FinancialSummary
            {
                TotalIncome = transactions.Where(t => t.Category == "Nhận").Sum(t => t.Amount),
                TotalExpense = transactions.Where(t => t.Category == "Chi").Sum(t => t.Amount),
                Balance = transactions.Sum(t => t.Category == "Nhận" ? t.Amount : -t.Amount),
                CategoryBreakdown = transactions
                    .GroupBy(t => t.Category)
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
            if (transaction != null && transaction.Category == "Nhận")
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveTransactions(List<Transaction> transactions)
        {
            var existingIds = _context.Transactions.Select(t => t.TransactionId).ToList();
            var newTransactions = transactions.Where(t => !existingIds.Contains(t.TransactionId)).ToList();
            _context.Transactions.AddRange(newTransactions);
            await _context.SaveChangesAsync();
        }
    }
}
