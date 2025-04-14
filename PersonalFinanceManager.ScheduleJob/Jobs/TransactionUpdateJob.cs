using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Models;
using PersonalFinanceManager.Shared.Services;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PersonalFinanceManager.Scheduler.Models;
using PersonalFinanceManager.Shared.Enum;
using DocumentFormat.OpenXml.InkML;
using PersonalFinanceManager.Shared.Constants;
using PersonalFinanceManager.Shared.Data.Entity;

namespace PersonalFinanceManager.Scheduler.Jobs
{
    public class TransactionUpdateJobDB : BaseJob, IJobConfiguration
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SchedulerConfig _config;
        private readonly ILogger<TransactionUpdateJobDB> _logger;
        private readonly string _basePath;

        public TransactionUpdateJobDB(
            IServiceProvider serviceProvider,
            ILogger<TransactionUpdateJobDB> logger,
            IOptions<SchedulerConfig> config)
            : base(logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
            // Lấy thư mục chứa file thực thi
            _basePath = AppContext.BaseDirectory;
            _logger.LogDebug($"Khởi tạo {BackgroundJobConstant.TransactionUpdateJobDb} với base path: {_basePath}");
        }

        public string JobName => BackgroundJobConstant.TransactionUpdateJobDb;
        public string JobType => "DB";
        public string CronSchedule => _config.Jobs.FirstOrDefault(j => j.JobName == JobName)?.CronSchedule ?? "0/5 * * * * ?";

        protected override async Task ExecuteJob(IJobExecutionContext context)
        {
            _logger.LogDebug($"Bắt đầu thực thi {BackgroundJobConstant.TransactionUpdateJobDb} tại {DateTime.Now}");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var gmailService = scope.ServiceProvider.GetRequiredService<GmailService>();

            try
            {
                var jobConfig = _config.Jobs.FirstOrDefault(j => j.JobName == JobName);
                _logger.LogDebug("Đọc cấu hình job: {Config}", JsonSerializer.Serialize(jobConfig));

                var parameters = string.IsNullOrEmpty(jobConfig?.ParametersJson)
                    ? new GmailJobParameters()
                    : JsonSerializer.Deserialize<GmailJobParameters>(jobConfig.ParametersJson);
                _logger.LogDebug("Tham số job: {Parameters}", JsonSerializer.Serialize(parameters));

                _logger.LogDebug("Kiểm tra kết nối database...");
                var canConnect = await dbContext.Database.CanConnectAsync();
                _logger.LogDebug("Kết nối database: {Status}", canConnect ? "Thành công" : "Thất bại");
                if (!canConnect)
                {
                    _logger.LogError("Không thể kết nối database với connection string: {ConnectionString}", dbContext.Database.GetConnectionString());
                    throw new InvalidOperationException("Không thể kết nối database.");
                }

                // Sử dụng đường dẫn tuyệt đối
                var credentialsPath = Path.Combine(_basePath, parameters.CredentialsPath ?? "Data/credentials.json");
                var tokenPath = Path.Combine(_basePath, parameters.TokenPath ?? "Data/token.json");

                _logger.LogDebug("Kiểm tra file credentials: {Path}", credentialsPath);
                if (!File.Exists(credentialsPath))
                {
                    _logger.LogError("File credentials không tồn tại: {Path}", credentialsPath);
                    throw new FileNotFoundException("Không tìm thấy credentials.json", credentialsPath);
                }
                _logger.LogDebug("Kiểm tra file token: {Path}", tokenPath);
                if (!File.Exists(tokenPath))
                {
                    _logger.LogWarning("File token không tồn tại, GmailService sẽ tạo mới nếu cần: {Path}", tokenPath);
                }

                _logger.LogDebug("Gọi GmailService...");
                var transactions = await gmailService.ExtractTransactionsAsync(
                    credentialsPath,
                    tokenPath,
                    parameters.MaxResults > 0 ? parameters.MaxResults : 10);

                _logger.LogInformation($"Đã lấy được {transactions.Count} giao dịch.");

                await SaveTransactions(dbContext, transactions);

                _logger.LogDebug("Hoàn tất TransactionUpdateJobDB tại {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực thi TransactionUpdateJobDB: {Message}", ex.Message);
                throw;
            }
        }

        private async Task SaveTransactions(AppDbContext dbContext, List<Transaction> transactions)
        {
            _logger.LogDebug("Bắt đầu lưu {Count} giao dịch vào database...", transactions.Count);

            var existingIds = await dbContext.Transactions
                .Select(t => t.TransactionId)
                .ToListAsync();
            var newTransactions = transactions
                .Where(t => !existingIds.Contains(t.TransactionId))
                .ToList();

            if (newTransactions.Any())
            {
                _logger.LogDebug("Phân loại {Count} giao dịch mới...", newTransactions.Count);
                newTransactions = await ClassifyTransactionsAsync(dbContext, newTransactions);
                dbContext.Transactions.AddRange(newTransactions);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Đã lưu {Count} giao dịch mới.", newTransactions.Count);
            }
            else
            {
                _logger.LogInformation("Không có giao dịch mới để lưu.");
            }
        }

        private async Task<List<Transaction>> ClassifyTransactionsAsync(AppDbContext dbContext, List<Transaction> transactions)
        {
            _logger.LogDebug("Bắt đầu phân loại giao dịch...");

            var labelingRules = await dbContext.LabelingRules.ToListAsync();
            var categories = await dbContext.Categories.ToListAsync();
            var transactionTypes = await dbContext.TransactionTypes.ToListAsync();

            var defaultCategory = categories.FirstOrDefault(c => c.Code == CategoryCodes.SINH_HOAT); 
            var defaultTransactionType = dbContext.TransactionTypes.FirstOrDefault(t => t.Code == TransactionTypeConstant.Expense);

            if (defaultCategory == null)
            {
                _logger.LogError("Không tìm thấy danh mục mặc định GT-PS.");
                throw new InvalidOperationException("Danh mục mặc định GT-PS không tồn tại.");
            }
            if (defaultTransactionType == null)
            {
                _logger.LogError("Không tìm thấy loại giao dịch mặc định EXPENSE.");
                throw new InvalidOperationException("Loại giao dịch mặc định EXPENSE không tồn tại.");
            }

            foreach (var tx in transactions)
            {
                if (string.IsNullOrWhiteSpace(tx.Description))
                {
                    _logger.LogDebug("Bỏ qua giao dịch không có mô tả: {TransactionId}", tx.TransactionId);
                    tx.CategoryId = defaultCategory.Id;
                    tx.TransactionTypeId = defaultTransactionType.Id;
                    tx.Status = (int)TransactionStatusEnum.Success;
                    continue;
                }

                var matchedRule = labelingRules.FirstOrDefault(rule =>
                    tx.Description.Contains(rule.Keyword, StringComparison.OrdinalIgnoreCase));

                if (matchedRule != null)
                {
                    tx.CategoryId = matchedRule.CategoryId;
                    tx.TransactionTypeId = matchedRule.TransactionTypeId;
                    _logger.LogDebug("Giao dịch {TransactionId} khớp rule {Keyword}: CategoryId={CategoryId}, TransactionTypeId={TransactionTypeId}",
                        tx.TransactionId, matchedRule.Keyword, tx.CategoryId, tx.TransactionTypeId);
                }
                else
                {
                    tx.CategoryId = defaultCategory.Id;
                    tx.TransactionTypeId = defaultTransactionType.Id;
                    _logger.LogDebug("Giao dịch {TransactionId} dùng mặc định: CategoryId={CategoryId}, TransactionTypeId={TransactionTypeId}",
                        tx.TransactionId, tx.CategoryId, tx.TransactionTypeId);
                }

                tx.Status = tx.TransactionTypeId == (int)TransactionTypeEnum.Advance
                    ? (int)TransactionStatusEnum.Pending
                    : (int)TransactionStatusEnum.Success;
            }

            _logger.LogDebug("Hoàn tất phân loại {Count} giao dịch.", transactions.Count);
            return transactions;
        }
    }
}