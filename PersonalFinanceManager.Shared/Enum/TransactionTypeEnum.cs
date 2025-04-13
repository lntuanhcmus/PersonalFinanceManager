using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Enum
{
    public enum TransactionTypeEnum
    {
        [Display(Name = "Thu Nhập")]
        Income = 1,

        [Display(Name = "Chi Trả")]
        Expense = 2,

        [Display(Name = "Tạm Ứng")]
        Advance = 3
    }
}
