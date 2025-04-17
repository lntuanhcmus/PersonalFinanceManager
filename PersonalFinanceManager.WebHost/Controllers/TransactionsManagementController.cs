using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.WebHost.Helper;
using PersonalFinanceManager.WebHost.Models;
using System.Globalization;
using System.Text;
using X.PagedList;

namespace PersonalFinanceManager.WebHost.Controllers
{
    [Authorize]
    public class TransactionsManagementController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        public TransactionsManagementController(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string transactionId = null,
            string startDate = null,
            string endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            int? categoryId = null,
            int? transactionTypeId = null,
            string sourceAccount = null,
            string content = null,
            int? status = null,
            int page = 1)
        {

            DateTime? startDateValue = startDate == null ? null : DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime? endDateValue = endDate == null ? null : DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            
            var client = _httpClientFactory.CreateClient("ApiClient");
            var query = $"?transactionId={Uri.EscapeDataString(transactionId ?? "")}" +
                        $"&startDate={(startDateValue.HasValue ? startDateValue.Value.ToString("yyyy-MM-dd") : "")}" +
                        $"&endDate={(endDateValue.HasValue ? endDateValue.Value.AddDays(1).ToString("yyyy-MM-dd") : "")}" +
                        $"&minAmount={minAmount}" +
                        $"&maxAmount={maxAmount}" +
                        $"&categoryId={categoryId}" +
                        $"&transactionTypeId={transactionTypeId}" +
                        $"&sourceAccount={Uri.EscapeDataString(sourceAccount ?? "")}" +
                        $"&content={Uri.EscapeDataString(content ?? "")}" +
                        $"&status={status}" +
                        $"&page={page}&pageSize=10";
            var response = await client.GetAsync($"api/TransactionsApi{query}");

            IPagedList<TransactionDto> pagedTransactions;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var pagedResponse = JsonConvert.DeserializeObject<PagedResponse<TransactionDto>>(json);
                pagedTransactions = new StaticPagedList<TransactionDto>(
                    pagedResponse.Items,
                    pagedResponse.PageNumber,
                    pagedResponse.PageSize,
                    pagedResponse.TotalItems);
            }
            else
            {
                pagedTransactions = new StaticPagedList<TransactionDto>(new List<TransactionDto>(), page, 10, 0);
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
                CategoryId = categoryId,
                SourceAccount = sourceAccount,
                Content = content,
                Status = status,
                Categories = await PopulateCategories(),
                TransactionTypes = await PopulateTransctionTypes(),
                Statuses = await PopulateStatuses(),
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Export(
            string transactionId = null,
            string startDate = null,
            string endDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            int? categoryId = null,
            int? transactionTypeId = null,
            string sourceAccount = null,
            string content = null,
            int? status = null,
            int page = 1,
            string? successMessage = null,
            string? errorMessage = null)
        {
            if(!string.IsNullOrEmpty(successMessage))
            {
                TempData["Success"] = successMessage;
            }
            if(!string.IsNullOrEmpty(errorMessage))
            {
                TempData["Error"] = errorMessage;
            }
            DateTime? startDateValue = startDate == null ? null : DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime? endDateValue = endDate == null ? null : DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // Tạo query string
            var client = _httpClientFactory.CreateClient("ApiClient");

            var query = $"?transactionId={Uri.EscapeDataString(transactionId ?? "")}" +
                        $"&startDate={(startDateValue.HasValue ? startDateValue.Value.ToString("yyyy-MM-dd") : "")}" +
                        $"&endDate={(endDateValue.HasValue ? endDateValue.Value.AddDays(1).ToString("yyyy-MM-dd") : "")}" +
                        $"&minAmount={minAmount}" +
                        $"&maxAmount={maxAmount}" +
                        $"&categoryId={categoryId}" +
                        $"&transactionTypeId={transactionTypeId}" +
                        $"&sourceAccount={Uri.EscapeDataString(sourceAccount ?? "")}" +
                        $"&content={Uri.EscapeDataString(content ?? "")}" +
                        $"&status={status}";

            var response = await client.GetAsync($"api/transactionsApi/export{query}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể xuất dữ liệu ra excel";
                return RedirectToAction("Index");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var fileName = $"transactions_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(fileBytes, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> AddTransaction()
        {
            var model = new DetailTransactionViewModel
            {
                TransactionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                Categories = await PopulateCategories(),
                TransactionTypes = await PopulateTransctionTypes()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction(DetailTransactionViewModel model)
        {
            model.Categories = await PopulateCategories();
            model.TransactionTypes = await PopulateTransctionTypes();
            if (ModelState.IsValid)
            {
                if (model.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Số tiền phải lớn hơn 0.");
                    return View(model);
                }

                if (model.CategoryId == null)
                {
                    ModelState.AddModelError("Category", "Loại giao dịch phải là bắt buộc.");
                    return View(model);
                }



                var client = _httpClientFactory.CreateClient("ApiClient");
                var checkDuplicate = await client.GetAsync($"api/transactionsApi/get-by-id?id={model.TransactionId}");
                if (checkDuplicate.IsSuccessStatusCode)
                {
                    var existedTransaction = JsonConvert.DeserializeObject<Transaction>(await checkDuplicate.Content.ReadAsStringAsync());
                    if(existedTransaction != null)
                    {
                        ModelState.AddModelError("TransactionId", "Mã giao dịch đã tồn tại.");
                        return View(model);
                    } 

                }


                var transaction = _mapper.Map<TransactionDto>(model);

                var json = JsonConvert.SerializeObject(transaction);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/transactionsApi", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Giao dịch đã được thêm thành công";
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Không thể thêm giao dịch. Vui lòng thử lại.");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditTransaction(string id)
        {

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/transactionsApi/get-by-id?id={id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var transaction = JsonConvert.DeserializeObject<TransactionDto>(json);
                if (transaction != null)
                {
                    var model = _mapper.Map<DetailTransactionViewModel>(transaction);
                    model.Categories = await PopulateCategories();
                    model.TransactionTypes = await PopulateTransctionTypes();
                    model.Statuses = await PopulateStatuses();
                    return View(model);
                }

            }
            TempData["Error"] = "Không tìm thấy giao dịch";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditTransaction(DetailTransactionViewModel model)
        {
            model.TransactionTypes = await PopulateTransctionTypes();
            model.Categories = await PopulateCategories();

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var transactionDto = _mapper.Map<TransactionDto>(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = JsonConvert.SerializeObject(transactionDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/transactionsApi/{transactionDto.TransactionId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Giao dịch đã được cập nhật thành công!";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Lỗi khi cập nhật giao dịch";
            return View(model);
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

        [HttpGet]
        public async Task<IActionResult> GetRepaymentTransaction(string id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/repaymentTransactionsApi/?transactionId={id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var transaction = JsonConvert.DeserializeObject<RepaymentTransactionDto>(json);
                if (transaction != null)
                {
                    return Ok(transaction);
                }
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetRepaymentTransactionsByTransactionId(string transactionId)
        {
            var transaction = new List<RepaymentTransactionDto>();
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/repaymentTransactionApi/?transactionId={transactionId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                transaction = JsonConvert.DeserializeObject<List<RepaymentTransactionDto>>(json);
            }

            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> AddRepaymentTransaction([FromBody] RepaymentTransactionDto transactionDto)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonConvert.SerializeObject(transactionDto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/RepaymentTransactionApi", content);

            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<dynamic>(json);
                return BadRequest(errorObj?.error?.ToString() ?? "Lỗi khi thêm giao dịch hoàn trả.");
            }

            return Ok(JsonConvert.DeserializeObject<object>(json));
        }

        [HttpPut]
        public async Task<IActionResult> EditRepaymentTransaction([FromBody] RepaymentTransactionDto transactionDto)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonConvert.SerializeObject(transactionDto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/RepaymentTransactionApi/{transactionDto.Id}", content);

            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<dynamic>(json);
                return BadRequest(errorObj?.error?.ToString() ?? "Lỗi khi cập nhật giao dịch hoàn trả.");
            }

            return Ok(JsonConvert.DeserializeObject<object>(json));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRepaymentTransaction(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.DeleteAsync($"api/RepaymentTransactionApi/{id}");

            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<dynamic>(json);
                return BadRequest(errorObj?.error?.ToString() ?? "Lỗi khi xóa giao dịch hoàn trả.");
            }

            return Ok(JsonConvert.DeserializeObject<object>(json));
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
        [AllowAnonymous]
        public async Task<IActionResult> InitiateOAuth()
        {
            try
            {
                // Tạo client HTTP
                var client = _httpClientFactory.CreateClient("ApiClient");

                // Gửi yêu cầu tới API đích
                var response = await client.GetAsync("api/gmailApi/initiate-oauth");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorObj = JsonConvert.DeserializeObject<object>(json);
                    return BadRequest(new { error = errorObj?.ToString() ?? "Lỗi khi gọi API Gmail" });
                }

                // Phân tích kết quả từ API
                return Ok(json);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi gọi API: {ex.Message}" });
            }
            catch (JsonException)
            {
                return BadRequest(new { error = "Không thể phân tích phản hồi từ API" });
            }
        }

        private async Task<List<SelectListItem>>  PopulateTransctionTypes()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var transactionTypesData = await client.GetFromJsonAsync<List<TransactionType>>($"api/transactionsApi/get-transaction-types");

            var transactionTypeList = transactionTypesData
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            return transactionTypeList;

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

        private async Task<List<SelectListItem>> PopulateStatuses()
        {
            return EnumHelper.GetSelectListWithValue<TransactionStatusEnum>();

        }


    }
}
