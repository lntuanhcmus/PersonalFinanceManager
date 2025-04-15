using PersonalFinanceManager.Shared.Data;
using System.Globalization;

namespace PersonalFinanceManager.Shared.Dto
{
    public class RepaymentTransactionDto
    {
        public RepaymentTransactionDto()
        {

        }
        public RepaymentTransactionDto(RepaymentTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            Id = transaction.Id;

            TransactionId = transaction.TransactionId;

            CreatedAt = transaction.CreatedAt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            Amount = transaction.Amount;

            Description = transaction.Description;

            SenderName = transaction.SenderName;

            Description = transaction.Description;

            TransactionTime = transaction.TransactionTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture); ;
        }
        
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public string? CreatedAt { get; set; }

        public decimal Amount { get; set; }

        public string SenderName { get; set; }

        public string Description { get; set; }

        public string TransactionTime { get; set; }

        public RepaymentTransaction RevertTransactionFromDto()
        {
            var repaymentTransaction = new RepaymentTransaction()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = !String.IsNullOrEmpty(CreatedAt) ? DateTime.ParseExact(CreatedAt, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : DateTime.Now,
                TransactionTime =  DateTime.ParseExact(TransactionTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                Description = Description,
                SenderName = SenderName,
            };

            return repaymentTransaction;
        }
    }
}
