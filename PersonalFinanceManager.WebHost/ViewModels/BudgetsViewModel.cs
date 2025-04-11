using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Models;
using X.PagedList;

namespace PersonalFinanceManager.WebHost.Models
{
    public class BudgetsViewModel
    {
        public IPagedList<BudgetDto> PagedBudgets { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string Period { get; set; }
        public int? CategoryId { get; set; }
        public List<SelectListItem> Categories { get; set; }

        public List<SelectListItem> Periods { get; set; }
    }
}
