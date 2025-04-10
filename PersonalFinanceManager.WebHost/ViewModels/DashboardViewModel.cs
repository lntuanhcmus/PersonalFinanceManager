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
    }
}
