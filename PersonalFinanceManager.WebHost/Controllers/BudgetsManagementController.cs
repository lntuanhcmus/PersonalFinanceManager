using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.WebHost.Helper;
using PersonalFinanceManager.WebHost.Models;
using System.Globalization;
using System.Text;
using System.Text.Json;
using X.PagedList;
using X.PagedList.Extensions;

namespace PersonalFinanceManager.WebHost.Controllers
{
    public class BudgetsManagementController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BudgetsManagementController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? categoryId = null,
            string startDate = null,
            string endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string period = null,
            int page = 1)
        {
            DateTime? startDateValue = string.IsNullOrEmpty(startDate) ? null : DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime? endDateValue = string.IsNullOrEmpty(endDate) ? null : DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var query = $"?categoryId={categoryId}" +
                        $"&startDate={(startDateValue.HasValue ? startDateValue.Value.ToString("yyyy-MM-dd") : "")}" +
                        $"&endDate={(endDateValue.HasValue ? endDateValue.Value.AddDays(1).ToString("yyyy-MM-dd") : "")}" +
                        $"&minAmount={minAmount}" +
                        $"&maxAmount={maxAmount}" +
                        $"&period={Uri.EscapeDataString(period ?? "")}" +
                        $"&page={page}&pageSize=10";

            var response = await client.GetAsync($"api/BudgetsApi{query}");

            IPagedList<BudgetDto> pagedBudgets;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var pagedResponse = JsonSerializer.Deserialize<PagedResponse<BudgetDto>>(json, options);
                pagedBudgets = new StaticPagedList<BudgetDto>(
                    pagedResponse.Items,
                    pagedResponse.PageNumber,
                    pagedResponse.PageSize,
                    pagedResponse.TotalItems);
            }
            else
            {
                pagedBudgets = new StaticPagedList<BudgetDto>(new List<BudgetDto>(), page, 10, 0);
            }

            var model = new BudgetsViewModel
            {
                PagedBudgets = pagedBudgets,
                CategoryId = categoryId,
                StartDate = startDateValue,
                EndDate = endDateValue,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                Period = period,
                Categories = await PopulateCategories(),
                Periods = await PopulatePeriods()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new DetailBudgetViewModel
            {
                StartDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                Categories = await PopulateCategories(),
                Periods = await PopulatePeriods()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DetailBudgetViewModel model)
        {
            model.Categories = await PopulateCategories();
            model.Periods = await PopulatePeriods();
            if (ModelState.IsValid)
            {
                var budgetDto = new BudgetDto()
                {
                    Period = model.Period,
                    Amount = model.Amount,
                    CategoryId = model.CategoryId,
                    CategoryName = model.CategoryName,
                    Id = model.Id,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate
                };
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.PostAsJsonAsync("api/BudgetsApi", budgetDto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Ngân sách đã được thêm thành công!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Không thể thêm ngân sách.";
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var response = await client.GetAsync($"api/budgetsApi/get-by-id?id={id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var budgetDto = JsonSerializer.Deserialize<BudgetDto>(json, options);
                var model = new DetailBudgetViewModel()
                {
                    CategoryId = budgetDto.CategoryId,
                    Amount = budgetDto.Amount,
                    CategoryName = budgetDto.CategoryName,
                    EndDate = budgetDto.EndDate,
                    StartDate = budgetDto.StartDate,
                    Period = budgetDto.Period,
                    Id = id,
                    Categories = await PopulateCategories(),
                    Periods = await PopulatePeriods(),
                };
                if (budgetDto != null)
                {
                    return View(model);
                }
            }
            TempData["Error"] = "Không tìm thấy giao dịch";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DetailBudgetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Ánh xạ sang budgetDto để gửi API
            var budgetDto = new BudgetDto
            {
                Id = model.Id,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Period = model.Period,
                Amount = model.Amount,
                CategoryId = model.CategoryId,
                CategoryName = model.CategoryName
            };

            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = JsonSerializer.Serialize(budgetDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/budgetsApi/{budgetDto.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Ngân sách đã được cập nhật thành công!";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Lỗi khi cập nhật ngân sách";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            await client.DeleteAsync($"api/BudgetsApi/{id}");
            return RedirectToAction("Index");
        }

        private async Task<List<SelectListItem>> PopulateCategories()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var categoriesData = await client.GetFromJsonAsync<List<Category>>($"api/categoriesApi");

            var categoriesResult = categoriesData
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            return categoriesResult;

        }

        private async Task<List<SelectListItem>> PopulatePeriods()
        {
            return EnumHelper.GetSelectList<BudgetPeriodEnum>();
        }
    }
}