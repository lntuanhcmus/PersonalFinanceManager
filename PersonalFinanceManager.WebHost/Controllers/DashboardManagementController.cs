using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.WebHost.Models;
using System.Globalization;
using System.Text.Json;

namespace PersonalFinanceManager.WebHost.Controllers
{
    public class DashboardManagementController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DashboardManagementController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string startDate = null, string endDate = null)
        {
            // Nếu startDate không được cung cấp, mặc định là ngày đầu tháng hiện tại
            DateTime? parsedStartDate = string.IsNullOrEmpty(startDate)
                ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
                : DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // Nếu endDate không được cung cấp, để null
            DateTime? parsedEndDate = string.IsNullOrEmpty(endDate)
                ? null
                : DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var query = $"?startDate={(parsedStartDate.HasValue ? parsedStartDate.Value.ToString("yyyy-MM-dd") : "")}" +
                        $"&endDate={(parsedEndDate.HasValue ? parsedEndDate.Value.ToString("yyyy-MM-dd") : "")}";

            // Lấy summary
            var summaryResponse = await client.GetAsync($"api/TransactionsApi/summary{query}");
            FinancialSummary summary = summaryResponse.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<FinancialSummary>(await summaryResponse.Content.ReadAsStringAsync()) ?? new FinancialSummary()
                : new FinancialSummary();

            // Lấy transactions
            var transactionsResponse = await client.GetAsync($"api/TransactionsApi{query}");
            var transactions = transactionsResponse.IsSuccessStatusCode
                ? (await transactionsResponse.Content.ReadFromJsonAsync<PagedResponse<TransactionDto>>()).Items
                : new List<TransactionDto>();

            // Lấy budgets
            var budgetsResponse = await client.GetAsync("api/BudgetsApi");
            var budgets = budgetsResponse.IsSuccessStatusCode
                ? (await budgetsResponse.Content.ReadFromJsonAsync<PagedResponse<BudgetDto>>()).Items
                : new List<BudgetDto>();

            var model = new DashboardViewModel
            {
                Summary = summary,
                StartDate = parsedStartDate,
                EndDate = parsedEndDate,
                TransactionsDto = transactions,
                BudgetsDto = budgets
            };

            return View(model);
        }
    }
}