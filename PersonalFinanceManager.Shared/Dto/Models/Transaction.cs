using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalFinanceManager.Shared.Models
{
    public class Transaction
    {
        [Key]
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonPropertyName("transactionTime")]
        public DateTime TransactionTime { get; set; }

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

        [JsonPropertyName("category")]
        public string Category { get; set; }
    }
}