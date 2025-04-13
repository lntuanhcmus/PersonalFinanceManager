using PersonalFinanceManager.Shared.Models;
using System.Globalization;

namespace PersonalFinanceManager.Shared.Dto
{
    public class TransactionDto
    {
        public TransactionDto()
        {

        }
        public TransactionDto(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            TransactionId = transaction.TransactionId;

            TransactionTime = transaction.TransactionTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            SourceAccount = transaction.SourceAccount;

            RecipientAccount = transaction.RecipientAccount;

            RecipientName = transaction.RecipientName;

            RecipientBank = transaction.RecipientBank;

            Amount = transaction.Amount;

            Description = transaction.Description;

            CategoryId = transaction.CategoryId;

            TransactionTypeId = transaction.TransactionTypeId;

            // Navigation properties (có thể null nếu không include)
            TransactionTypeName = transaction.TransactionType?.Name;

            CategoryName = transaction.Category?.Name;

            Status = transaction.Status;

            RepaymentAmount = transaction.RepaymentAmount;
        }
        public string TransactionId { get; set; }

        public string TransactionTime { get; set; } // Nhận dưới dạng chuỗi

        public string SourceAccount { get; set; }

        public string RecipientAccount { get; set; }

        public string RecipientName { get; set; }

        public string RecipientBank { get; set; }

        public decimal Amount { get; set; }

        public decimal RepaymentAmount { get; set; }

        public string Description { get; set; }

        public int? CategoryId { get; set; }

        public int TransactionTypeId { get; set; }

        public string? TransactionTypeName { get; set; }

        public string? CategoryName { get; set; }

        public int Status { get; set; }
    }
}
