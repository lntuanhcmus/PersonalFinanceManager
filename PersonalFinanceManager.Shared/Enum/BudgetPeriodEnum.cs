using System.ComponentModel.DataAnnotations;

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
