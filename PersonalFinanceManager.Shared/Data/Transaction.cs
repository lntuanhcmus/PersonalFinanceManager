using System.ComponentModel.DataAnnotations;
namespace PersonalFinanceManager.Shared.Data
{

    public class Transaction
    {
        [Key]
        public string TransactionId { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime TransactionTime { get; set; }

        public int? CategoryId { get; set; }

        public Category Category { get; set; }

        public int TransactionTypeId { get; set; }

        public TransactionType TransactionType { get; set; }

        public string SourceAccount { get; set; }

        public string RecipientAccount { get; set; }

        public string RecipientName { get; set; }

        public string RecipientBank { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public decimal RepaymentAmount { get; set; } = 0;

        public int Status { get; set; } = 1;

        public bool NeedsManualReview { get; set; }



        public ICollection<RepaymentTransaction> RepaymentTransactions { get; set; }

    }
}