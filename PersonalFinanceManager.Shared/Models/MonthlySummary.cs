using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Models
{
    public class MonthlySummary
    {
        [JsonPropertyName("income")]
        public decimal Income { get; set; }

        [JsonPropertyName("expense")]
        public decimal Expense { get; set; }
    }
}
