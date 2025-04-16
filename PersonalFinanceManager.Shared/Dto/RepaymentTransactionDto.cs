using PersonalFinanceManager.Shared.Data;
using System.Globalization;

namespace PersonalFinanceManager.Shared.Dto
{
    public class RepaymentTransactionDto
    {
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public string? CreatedAt { get; set; }

        public decimal Amount { get; set; }

        public string SenderName { get; set; }

        public string Description { get; set; }

        public string TransactionTime { get; set; }
    }
}
