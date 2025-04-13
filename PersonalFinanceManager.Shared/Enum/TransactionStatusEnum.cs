using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Enum
{
    public enum TransactionStatusEnum
    {
        [Display(Name = "Hoàn Thành")]
        Success = 1,

        [Display(Name = "Chờ giải quyết")]
        Pending = 2,

        [Display(Name = "Thất bại")]
        Failed = 0
    }
}
