using PersonalFinanceManager.Shared.Data;
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

        public int Id { get; set; }

        public int CategoryId { get; set; } // "Chi - Ăn uống", "Chi - Mua sắm", ...

        public string? CategoryName { get; set; }

        public decimal Amount { get; set; } // Số tiền tối đa cho ngân sách

        public string Period { get; set; } // "Monthly", "Yearly" (chu kỳ ngân sách)

        public string StartDate { get; set; } // Ngày bắt đầu ngân sách

        public string EndDate { get; set; } // Ngày kết thúc (null nếu không giới hạn)

    }
}
