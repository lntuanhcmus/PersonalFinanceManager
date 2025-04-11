using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Models;
using System.Text.Json.Serialization;

namespace PersonalFinanceManager.WebHost.Models
{
    public class DashboardViewModel
    {
        [JsonPropertyName("summary")]
        public FinancialSummary Summary { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("transactionsDto")]
        public List<TransactionDto> TransactionsDto { get; set; } // Thêm để tính toán chi tiêu

        [JsonPropertyName("budgetsDto")]
        public List<BudgetDto> BudgetsDto { get; set; } // Danh sách ngân sách
    }

    public class BudgetUsageViewModel
    {
        public string CategoryName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
    }
}
