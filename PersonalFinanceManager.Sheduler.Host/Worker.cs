using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalFinanceManager.Scheduler.Services;
using System.Threading;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Scheduler.Host
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SchedulerService _schedulerService;

        public Worker(ILogger<Worker> logger, SchedulerService schedulerService)
        {
            _logger = logger;
            _schedulerService = schedulerService;
            _logger.LogInformation("Worker khởi tạo.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker bắt đầu chạy tại: {Time}", DateTimeOffset.Now);
            await _schedulerService.StartAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker dừng tại: {Time}", DateTimeOffset.Now);
            await _schedulerService.StopAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}