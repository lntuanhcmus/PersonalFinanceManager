using System.Text.Json.Serialization;
using System.Collections.Generic;
using PersonalFinanceManager.Shared.Models;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinanceManager.Shared.Dto;

namespace PersonalFinanceManager.WebHost.Models
{
    public class TransactionsViewModel
    {
        [JsonPropertyName("transactions")]
        public List<TransactionDto> Transactions { get; set; } // Giữ để tương thích view cũ

        [JsonPropertyName("pagedTransactions")]
        public IPagedList<TransactionDto> PagedTransactions { get; set; } // Dùng cho phân trang

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

        [JsonPropertyName("sourceAccount")]
        public string SourceAccount { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("categoryId")]
        public int? CategoryId { get; set; }

        [JsonPropertyName("transactionTypeId")]
        public int? TransactionTypeId { get; set; }

        [JsonPropertyName("categories")]
        public List<SelectListItem> Categories { get; set; }

        [JsonPropertyName("transactionTypes")]
        public List<SelectListItem> TransactionTypes { get; set; }

    }
}