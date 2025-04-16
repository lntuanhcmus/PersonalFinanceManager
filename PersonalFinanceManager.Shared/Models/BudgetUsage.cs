using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Models
{
    public class BudgetUsage
    {
        public string CategoryName { get; set; } = string.Empty;

        public decimal BudgetAmount { get; set; }

        public decimal SpentAmount { get; set; }
    }
}
