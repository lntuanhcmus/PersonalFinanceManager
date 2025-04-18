﻿using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinanceManager.Shared.Dto;

namespace PersonalFinanceManager.WebHost.Models
{
    public class DetailBudgetViewModel: BudgetDto
    {
        public List<SelectListItem>? Categories { get; set; }

        public List<SelectListItem>? Periods { get; set; }

    }
}
