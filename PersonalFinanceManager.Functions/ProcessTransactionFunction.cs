using System;
using System.Text;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PersonalFinanceManager.Infrastructure.Data;
using PersonalFinanceManager.Shared.Constants;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Enum;

namespace PersonalFinanceManager.Functions
{
    public class ProcessTransactionFunction
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ProcessTransactionFunction> _logger;

        public ProcessTransactionFunction(AppDbContext dbContext, ILogger<ProcessTransactionFunction> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("ProcessTransaction")]
        public async Task Run([QueueTrigger("transaction-queue", Connection = "AzureWebJobsStorage")] string message)
        {
            try
            {
                var transaction = JsonConvert.DeserializeObject<Transaction>(
    Encoding.UTF8.GetString(Convert.FromBase64String(message.Trim('"'))));

                _logger.LogInformation("Processing transaction for user {UserId}: {TransactionId}", transaction.TransactionId, transaction.UserId);

                // Check duplicate
                var existingTransaction = await _dbContext.Transactions
                        .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId);
                if (existingTransaction != null)
                {
                    _logger.LogWarning("Transaction {TransactionId} already exists, skipping",
                        transaction.TransactionId);
                    return;
                }

                // Phân loại giao dịch
                await ClassifyTransactionAsync(transaction);

                // Lưu vào database
                _dbContext.Transactions.Add(transaction);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Transaction saved: {TransactionId}, CategoryId: {CategoryId}, TransactionTypeId: {TransactionTypeId}",
                    transaction.TransactionId, transaction.CategoryId, transaction.TransactionTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transaction from queue");
                throw; // Để queue retry
            }
        }

        private async Task ClassifyTransactionAsync(Transaction transaction)
        {
            if (string.IsNullOrWhiteSpace(transaction.Description))
            {
                _logger.LogWarning("Transaction {TransactionId} has empty description, using default values",
                    transaction.TransactionId);
                await AssignDefaultValuesAsync(transaction);
                return;
            }

            // Lấy LabelingRules
            var labelingRules = await _dbContext.LabelingRules.ToListAsync();

            // Tìm rule khớp
            var matchedRule = labelingRules
                .FirstOrDefault(rule => transaction.Description.Contains(rule.Keyword, StringComparison.OrdinalIgnoreCase));

            if (matchedRule != null)
            {
                transaction.CategoryId = matchedRule.CategoryId;
                transaction.TransactionTypeId = matchedRule.TransactionTypeId;
                _logger.LogInformation("Matched rule for transaction {TransactionId}: Keyword={Keyword}, CategoryId={CategoryId}",
                    transaction.TransactionId, matchedRule.Keyword, matchedRule.CategoryId);
            }
            else
            {
                _logger.LogInformation("No rule matched for transaction {TransactionId}, using default values",
                    transaction.TransactionId);
                await AssignDefaultValuesAsync(transaction);
            }

            // Gán Status
            transaction.Status = transaction.TransactionTypeId == (int)TransactionTypeEnum.Advance
                ? (int)TransactionStatusEnum.Pending
                : (int)TransactionStatusEnum.Success;
        }

        private async Task AssignDefaultValuesAsync(Transaction transaction)
        {
            // Lấy default Category và TransactionType
            var categories = await _dbContext.Categories.ToListAsync();
            var defaultCategory = categories.FirstOrDefault(c => c.Code == CategoryCodes.SINH_HOAT);
            if (defaultCategory == null)
            {
                _logger.LogError("Default category GT-PS not found");
                throw new InvalidOperationException("Default category GT-PS not found");
            }

            var defaultTransactionType = await _dbContext.TransactionTypes
                .FirstOrDefaultAsync(t => t.Code == TransactionTypeConstant.Expense);
            if (defaultTransactionType == null)
            {
                _logger.LogError("Default transaction type Expense not found");
                throw new InvalidOperationException("Default transaction type Expense not found");
            }

            transaction.CategoryId = defaultCategory.Id;
            transaction.TransactionTypeId = defaultTransactionType.Id;
        }
    }
}
