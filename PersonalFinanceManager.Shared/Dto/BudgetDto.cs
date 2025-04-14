using PersonalFinanceManager.Shared.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Dto
{
    public class BudgetDto
    {
        public BudgetDto() { }
        public BudgetDto(Budget budget)
        {
            Id = budget.Id;
            CategoryId = budget.CategoryId;
            CategoryName = budget?.Category?.Name;
            Amount = budget.Amount;
            Period = budget.Period;
            StartDate = budget.StartDate.ToString("dd/MM/yyyy");
            EndDate = budget.EndDate != null ? budget.EndDate.Value.ToString("dd/MM/yyyy") : null;
        }
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; } // "Chi - Ăn uống", "Chi - Mua sắm", ...

        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; } // Số tiền tối đa cho ngân sách

        [JsonPropertyName("period")]
        public string Period { get; set; } // "Monthly", "Yearly" (chu kỳ ngân sách)

        [JsonPropertyName("startDate")]
        public string StartDate { get; set; } // Ngày bắt đầu ngân sách

        [JsonPropertyName("endDate")]
        public string EndDate { get; set; } // Ngày kết thúc (null nếu không giới hạn)

    }
}
