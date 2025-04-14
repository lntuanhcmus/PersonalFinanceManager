﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using PersonalFinanceManager.Scheduler.Jobs;
using PersonalFinanceManager.Scheduler.Models;
using PersonalFinanceManager.Shared.Constants;
using PersonalFinanceManager.Shared.Data.Entity;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Helpers;
using PersonalFinanceManager.Shared.ML.Models;
using Quartz;
using PersonalFinanceManager.Shared.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class TransactionUpdateJobDB : BaseJob, IJobConfiguration
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SchedulerConfig _config;
    private readonly ILogger<TransactionUpdateJobDB> _logger;
    private readonly string _basePath;
    private readonly MLContext _mlContext;
    private readonly ITransformer _categoryModel;
    private readonly ITransformer _typeModel;
    private readonly List<Category> _categories;
    private readonly List<TransactionType> _transactionTypes;

    public TransactionUpdateJobDB(
        IServiceProvider serviceProvider,
        ILogger<TransactionUpdateJobDB> logger,
        IOptions<SchedulerConfig> config)
        : base(logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
        _basePath = AppContext.BaseDirectory;
        _logger.LogInformation($"Khởi tạo {BackgroundJobConstant.TransactionUpdateJobDb} với base path: {_basePath}");

        try
        {
            _mlContext = new MLContext();
            var categoryModelPath = Path.Combine(_basePath, "Data/categoryModel.zip");
            var typeModelPath = Path.Combine(_basePath, "Data/typeModel.zip");

            if (!File.Exists(categoryModelPath) || !File.Exists(typeModelPath))
            {
                _logger.LogWarning("Không tìm thấy mô hình tại {CategoryPath} hoặc {TypePath}", categoryModelPath, typeModelPath);
                throw new FileNotFoundException("Không tìm thấy file mô hình ML.");
            }

            _logger.LogInformation("Tải mô hình category...");
            _categoryModel = _mlContext.Model.Load(categoryModelPath, out var categorySchema);
            _logger.LogInformation("Tải mô hình type...");
            _typeModel = _mlContext.Model.Load(typeModelPath, out var typeSchema);

            // Cache categories và transactionTypes
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                _categories = dbContext.Categories.ToList();
                _transactionTypes = dbContext.TransactionTypes.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi khởi tạo mô hình ML: {Message}", ex.Message);
            throw;
        }
    }

    public string JobName => BackgroundJobConstant.TransactionUpdateJobDb;
    public string JobType => "DB";
    public string CronSchedule => _config.Jobs.FirstOrDefault(j => j.JobName == JobName)?.CronSchedule ?? "0/5 * * * * ?";

    protected override async Task ExecuteJob(IJobExecutionContext context)
    {
        _logger.LogInformation($"Bắt đầu thực thi {BackgroundJobConstant.TransactionUpdateJobDb} tại {DateTime.Now}");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var gmailService = scope.ServiceProvider.GetRequiredService<GmailService>();

        try
        {
            var jobConfig = _config.Jobs.FirstOrDefault(j => j.JobName == JobName);
            _logger.LogInformation("Đọc cấu hình job: {Config}", JsonSerializer.Serialize(jobConfig));

            var parameters = string.IsNullOrEmpty(jobConfig?.ParametersJson)
                ? new GmailJobParameters()
                : JsonSerializer.Deserialize<GmailJobParameters>(jobConfig.ParametersJson);
            _logger.LogInformation("Tham số job: {Parameters}", JsonSerializer.Serialize(parameters));

            if (!parameters.IsEnabled)
            {
                _logger.LogInformation("TransactionUpdateJobDB is disabled. Skipping.");
                return;
            }

            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogError("Không thể kết nối database.");
                throw new InvalidOperationException("Không thể kết nối database.");
            }

            var credentialsPath = Path.Combine(_basePath, parameters.CredentialsPath ?? "Data/credentials.json");
            var tokenPath = Path.Combine(_basePath, parameters.TokenPath ?? "Data/token.json");

            if (!File.Exists(credentialsPath))
            {
                _logger.LogError("File credentials không tồn tại: {Path}", credentialsPath);
                throw new FileNotFoundException("Không tìm thấy credentials.json", credentialsPath);
            }

            var transactions = await gmailService.ExtractTransactionsAsync(
                credentialsPath,
                tokenPath,
                parameters.MaxResults > 0 ? parameters.MaxResults : 10);

            _logger.LogInformation($"Đã lấy được {transactions.Count()} giao dịch.");

            await SaveTransactions(dbContext, transactions);

            _logger.LogInformation("Hoàn tất TransactionUpdateJobDB tại {Time}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thực thi TransactionUpdateJobDB: {Message}", ex.Message);
            throw;
        }
    }

    private async Task SaveTransactions(AppDbContext dbContext, List<Transaction> transactions)
    {
        _logger.LogInformation("Bắt đầu lưu {Count} giao dịch...", transactions.Count);

        var existingIds = await dbContext.Transactions
            .Select(t => t.TransactionId)
            .ToListAsync();
        var newTransactions = transactions
            .Where(t => !existingIds.Contains(t.TransactionId))
            .ToList();

        if (newTransactions.Any())
        {
            _logger.LogInformation("Phân loại {Count} giao dịch mới...", newTransactions.Count);
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
        _logger.LogInformation("Bắt đầu phân loại {Count} giao dịch...", transactions.Count);

        var defaultCategory = _categories.FirstOrDefault(c => c.Code == CategoryCodes.SINH_HOAT);
        var defaultTransactionType = _transactionTypes.FirstOrDefault(t => t.Code == TransactionTypeConstant.Expense);

        if (defaultCategory == null || defaultTransactionType == null)
        {
            _logger.LogError("Không tìm thấy danh mục hoặc loại giao dịch mặc định.");
            throw new InvalidOperationException("Không tìm thấy danh mục hoặc loại giao dịch mặc định.");
        }

        // Chuyển thành batch TransactionData
        var mlInputs = transactions.Select(tx => new TransactionData
        {
            Description = string.IsNullOrWhiteSpace(tx.Description) ? "Unknown" : MLDataHelper.CleanText(tx.Description),
            Amount = (float)tx.Amount,
            TransactionTime = tx.TransactionTime.ToString("yyyy-MM-dd"),
            DayOfWeek = tx.TransactionTime.DayOfWeek.ToString(),
            HourOfDay = tx.TransactionTime.Hour,
            CategoryId = "",
            TransactionTypeId = ""
        }).ToList();

        var dataView = _mlContext.Data.LoadFromEnumerable(mlInputs);

        // Dự đoán batch
        var categoryPredictions = _categoryModel.Transform(dataView);
        var typePredictions = _typeModel.Transform(dataView);

        // Lấy kết quả dự đoán
        var categoryResults = _mlContext.Data.CreateEnumerable<TransactionPrediction>(
            categoryPredictions, reuseRowObject: false).ToList();
        var typeResults = _mlContext.Data.CreateEnumerable<TransactionPrediction>(
            typePredictions, reuseRowObject: false).ToList();

        // Gán nhãn cho giao dịch
        for (int i = 0; i < transactions.Count; i++)
        {
            var tx = transactions[i];
            var categoryPrediction = categoryResults[i];
            var typePrediction = typeResults[i];

            // Gán CategoryId
            bool isValidCategory = int.TryParse(categoryPrediction.CategoryId, out int predictedCategoryId) &&
                                  _categories.Any(c => c.Id == predictedCategoryId);
            tx.CategoryId = isValidCategory ? predictedCategoryId : defaultCategory.Id;
            _logger.LogInformation("Giao dịch {Id}: CategoryId={Category}", tx.TransactionId, tx.CategoryId);

            // Gán TransactionTypeId
            bool isValidType = int.TryParse(typePrediction.TransactionTypeId, out int predictedTypeId) &&
                               _transactionTypes.Any(t => t.Id == predictedTypeId);
            tx.TransactionTypeId = isValidType ? predictedTypeId : defaultTransactionType.Id;
            _logger.LogInformation("Giao dịch {Id}: TransactionTypeId={Type}", tx.TransactionId, tx.TransactionTypeId);

            // Gán Status
            tx.Status = tx.TransactionTypeId == (int)TransactionTypeEnum.Advance
                ? (int)TransactionStatusEnum.Pending
                : (int)TransactionStatusEnum.Success;
        }

        _logger.LogInformation("Hoàn tất phân loại {Count} giao dịch.", transactions.Count);
        return transactions;
    }
}
