using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.WebHost.Models;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using X.PagedList;

namespace PersonalFinanceManager.WebHost.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public TransactionsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string transactionId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string category = null,
            string sourceAccount = null,
            string content = null,
            int page = 1)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var query = $"?transactionId={Uri.EscapeDataString(transactionId ?? "")}" +
                        $"&startDate={(startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "")}" +
                        $"&endDate={(endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "")}" +
                        $"&minAmount={minAmount}" +
                        $"&maxAmount={maxAmount}" +
                        $"&category={Uri.EscapeDataString(category ?? "")}" +
                        $"&sourceAccount={Uri.EscapeDataString(sourceAccount ?? "")}" +
                        $"&content={Uri.EscapeDataString(content ?? "")}" +
                        $"&page={page}&pageSize=10";
            var response = await client.GetAsync($"api/transactionsApi{query}");

            IPagedList<Transaction> pagedTransactions;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var pagedResponse = JsonSerializer.Deserialize<PagedResponse<Transaction>>(json);
                pagedTransactions = new StaticPagedList<Transaction>(
                    pagedResponse.Items,
                    pagedResponse.PageNumber,
                    pagedResponse.PageSize,
                    pagedResponse.TotalItems);
            }
            else
            {
                pagedTransactions = new StaticPagedList<Transaction>(new List<Transaction>(), page, 10, 0);
            }

            var model = new TransactionsViewModel
            {
                Transactions = pagedTransactions.ToList(), // Để tương thích với view hiện tại
                PagedTransactions = pagedTransactions, // Thêm để dùng phân trang
                TransactionId = transactionId,
                StartDate = startDate,
                EndDate = endDate,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                Category = category,
                SourceAccount = sourceAccount,
                Content = content
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult AddReceivedTransaction()
        {
            var model = new Transaction
            {
                TransactionTime = DateTime.Now // Gán ngày giờ hiện tại
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddReceivedTransaction(TransactionDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/transactionsApi", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Giao dịch đã được thêm thành công!";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Lỗi khi thêm giao dịch";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditTransaction(string id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/transactionsApi/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var transaction = JsonSerializer.Deserialize<Transaction>(json, options);
                if (transaction != null)
                {
                    return View(transaction);
                }
            }
            TempData["Error"] = "Không tìm thấy giao dịch";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditTransaction(Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                return View(transaction);
            }

            // Ánh xạ sang TransactionDto để gửi API
            var transactionDto = new TransactionDto
            {
                TransactionId = transaction.TransactionId,
                TransactionTime = transaction.TransactionTime.ToString("MM/dd/yyyy HH:mm"),
                SourceAccount = transaction.SourceAccount,
                RecipientAccount = transaction.RecipientAccount,
                RecipientName = transaction.RecipientName,
                RecipientBank = transaction.RecipientBank,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Category = transaction.Category
            };

            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = JsonSerializer.Serialize(transactionDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/transactionsApi/{transaction.TransactionId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Giao dịch đã được cập nhật thành công!";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Lỗi khi cập nhật giao dịch";
            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.DeleteAsync($"api/transactionsApi/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Giao dịch đã được xóa thành công!";
            }
            else
            {
                TempData["Error"] = "Lỗi khi xóa giao dịch";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RefreshFromGmail()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsync("api/transactionsApi/refresh", null);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Lỗi khi lấy giao dịch từ Gmail";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? startDate, DateTime? endDate)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var query = "";
            if (startDate.HasValue) query += $"startDate={startDate.Value:yyyy-MM-dd}&";
            if (endDate.HasValue) query += $"endDate={endDate.Value:yyyy-MM-dd}";
            var url = $"api/transactionsApi/summary?{query.TrimEnd('&')}";
            var response = await client.GetAsync(url);

            FinancialSummary summary;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {json}");
                summary = JsonSerializer.Deserialize<FinancialSummary>(json) ?? new FinancialSummary
                {
                    TotalIncome = 0,
                    TotalExpense = 0,
                    Balance = 0,
                    CategoryBreakdown = new Dictionary<string, decimal>()
                };
            }
            else
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                summary = new FinancialSummary
                {
                    TotalIncome = 0,
                    TotalExpense = 0,
                    Balance = 0,
                    CategoryBreakdown = new Dictionary<string, decimal>()
                };
            }

            var model = new DashboardViewModel
            {
                Summary = summary,
                StartDate = startDate,
                EndDate = endDate
            };

            return View(model);
        }
    }
}
