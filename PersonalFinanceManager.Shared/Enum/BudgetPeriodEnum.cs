using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Enum
{
    public enum BudgetPeriodEnum
    {
        [Display(Name = "Hàng Tháng")]
        Monthly,

        [Display(Name = "Hàng Năm")]
        Yearly
    }
}
