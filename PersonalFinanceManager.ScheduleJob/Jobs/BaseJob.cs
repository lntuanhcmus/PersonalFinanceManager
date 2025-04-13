using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Scheduler.Jobs
{
    public abstract class BaseJob : IJob
    {
        protected readonly ILogger _logger;

        protected BaseJob(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"[{DateTime.Now}] Bắt đầu job {GetType().Name}...");
            try
            {
                await ExecuteJob(context);
                _logger.LogInformation($"[{DateTime.Now}] Job {GetType().Name} hoàn tất.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi trong job {GetType().Name}.");
                throw;
            }
        }

        protected abstract Task ExecuteJob(IJobExecutionContext context);
    }
}