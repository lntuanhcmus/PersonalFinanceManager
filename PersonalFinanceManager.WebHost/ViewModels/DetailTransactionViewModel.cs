using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinanceManager.Shared.Dto;

namespace PersonalFinanceManager.WebHost.Models
{
    public class DetailTransactionViewModel: TransactionDto
    {
        public List<SelectListItem> Categories { get; set; }
    }
}
