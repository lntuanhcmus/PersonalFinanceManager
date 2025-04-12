using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalFinanceManager.Shared.Models
{

    public class Transaction
    {
        [Key]
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("transactionTime")]
        public DateTime TransactionTime { get; set; }

        [JsonPropertyName("categoryId")]
        public int? CategoryId { get; set; }

        [JsonPropertyName("categoryId")]
        public Category Category { get; set; }

        [JsonPropertyName("transactionTypeId")]
        public int TransactionTypeId { get; set; }

        [JsonPropertyName("transactionType")]
        public TransactionType TransactionType { get; set; }

        [JsonPropertyName("sourceAccount")]
        public string SourceAccount { get; set; }

        [JsonPropertyName("recipientAccount")]
        public string RecipientAccount { get; set; }

        [JsonPropertyName("recipientName")]
        public string RecipientName { get; set; }

        [JsonPropertyName("recipientBank")]
        public string RecipientBank { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("relatedTransactionId")]
        public string? RelatedTransactionId { get; set; }

        [JsonPropertyName("relatedTransaction")]
        public Transaction? RelatedTransaction { get; set; } // Navigation property

    }
}