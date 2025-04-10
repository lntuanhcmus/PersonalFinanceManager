using System.Text.Json.Serialization;
using System.Collections.Generic;
using PersonalFinanceManager.Shared.Models;
using X.PagedList;

namespace PersonalFinanceManager.WebHost.Models
{
    public class TransactionsViewModel
    {
        [JsonPropertyName("transactions")]
        public List<Transaction> Transactions { get; set; } // Giữ để tương thích view cũ

        [JsonPropertyName("pagedTransactions")]
        public IPagedList<Transaction> PagedTransactions { get; set; } // Dùng cho phân trang

        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("minAmount")]
        public decimal? MinAmount { get; set; }

        [JsonPropertyName("maxAmount")]
        public decimal? MaxAmount { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("sourceAccount")]
        public string SourceAccount { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}