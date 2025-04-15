using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Shared.Data
{
    public class RepaymentTransaction
    {
        [Key]
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public Transaction Transaction { get; set; }

        public DateTime TransactionTime { get; set; }

        public DateTime CreatedAt { get; set; }

        public decimal Amount { get; set; }

        public string SenderName { get; set; }

        public string Description { get; set; }

    }
}
