using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.WebHost.Models
{
    public class TransactionsFilterModel
    {
        public string TransactionId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public decimal? MinAmount { get; set; }

        public decimal? MaxAmount { get; set; }

        public int? CategoryId { get; set; }

        public int? TransactionTypeId { get; set; }

        public string SourceAccount { get; set; }

        public string Content { get; set; }

        public int? Status { get; set; }

        // Paging
        public int? Page { get; set; } = 1;

        public int? PageSize { get; set; } = 10;
    }
}
