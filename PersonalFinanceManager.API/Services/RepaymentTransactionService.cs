using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Data.Entity;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Enum;

namespace PersonalFinanceManager.API.Services
{
    public class RepaymentTransactionService
    {
        private readonly AppDbContext _context;

        public RepaymentTransactionService(AppDbContext context)
        {
            _context = context;
        }

        public List<RepaymentTransaction> GetAll()
        {
            return _context.RepaymentTransactions
                .OrderByDescending(t => t.Id)
                .ToList();
        }

        public RepaymentTransaction GetById(int id)
        {
            return _context.RepaymentTransactions.FirstOrDefault(x => x.Id == id);
        }

        public async Task<List<RepaymentTransaction>> GetByTransactionId(string id)
        {
            return _context.RepaymentTransactions.Where(x => x.TransactionId == id).ToList();
        }

        public async Task AddTransaction(RepaymentTransaction transaction)
        {
            _context.RepaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            await CalculateTotalRepaymentAmount(transaction);
        }

        public async Task UpdateTransaction(RepaymentTransaction transaction)
        {
            _context.RepaymentTransactions.Update(transaction);
            await _context.SaveChangesAsync();

            await CalculateTotalRepaymentAmount(transaction);
        }

        public async Task DeleteTransaction(int id)
        {
            var transaction = _context.RepaymentTransactions.Find(id);
            if (transaction != null)
            {
                _context.RepaymentTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }

            await CalculateTotalRepaymentAmount(transaction);
        }

        private async Task CalculateTotalRepaymentAmount(RepaymentTransaction repaymentTransaction)
        {
            var transaction = _context.Transactions.FirstOrDefault(x=>x.TransactionId == repaymentTransaction.TransactionId);

            var totalRepaymentAmt = _context.RepaymentTransactions.Where(x => x.TransactionId == transaction.TransactionId).Sum(x => x.Amount);

            transaction.RepaymentAmount = totalRepaymentAmt;

            await _context.SaveChangesAsync();
        }
    }
}
