using PersonalFinanceManager.Shared.Models;

namespace PersonalFinanceManager.API.Helper
{
    public class TransactionHelper
    {
        public static decimal CalculateActualExpense(List<Transaction> transactions, int expenseTypeId, int advanceTypeId, int successStatus, int? categoryId = null, DateTime? start = null, DateTime? end = null)
        {
            var filtered = transactions
                .Where(t =>
                    (!start.HasValue || t.TransactionTime >= start) &&
                    (!end.HasValue || t.TransactionTime <= end) &&
                    (!categoryId.HasValue || t.CategoryId == categoryId))
                .ToList();

            var expense = filtered
                .Where(t => t.TransactionTypeId == expenseTypeId)
                .Sum(t => t.Amount);

            var advance = filtered
                .Where(t => t.TransactionTypeId == advanceTypeId && t.Status == successStatus)
                .Sum(t =>
                {
                    return t.Amount - t.RepaymentAmount;
                });

            return expense + advance;
        }
    }
}
