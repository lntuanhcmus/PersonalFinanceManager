using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Models
{
    public class FinancialSummary
    {
        [JsonPropertyName("totalIncome")]
        public decimal TotalIncome { get; set; }

        [JsonPropertyName("totalExpense")]
        public decimal TotalExpense { get; set; }

        [JsonPropertyName("totalAdvance")]
        public decimal TotalAdvance { get; set; }

        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }

        [JsonPropertyName("categoryBreakdown")]
        public Dictionary<int, decimal> CategoryBreakdown { get; set; }
    }
}
