using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PersonalFinanceManager.Shared.Models
{
    public class Budget
    {
        [Key]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; } // "Chi - Ăn uống", "Chi - Mua sắm", ...

        [JsonPropertyName("category")]
        public Category Category { get; set; } 

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; } // Số tiền tối đa cho ngân sách

        [JsonPropertyName("period")]
        public string Period { get; set; } // "Monthly", "Yearly" (chu kỳ ngân sách)

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; } // Ngày bắt đầu ngân sách

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; } // Ngày kết thúc (null nếu không giới hạn)
    }
}
