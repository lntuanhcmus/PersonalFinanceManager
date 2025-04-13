using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalFinanceManager.Scheduler.Jobs;
using PersonalFinanceManager.Scheduler.Models;
using PersonalFinanceManager.Scheduler.Services;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Services;

namespace PersonalFinanceManager.Scheduler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<Worker>();

                    // Đăng ký cấu hình
                    services.Configure<SchedulerConfig>(context.Configuration.GetSection("Scheduler"));

                    // Đăng ký DbContext
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(context.Configuration.GetSection("Scheduler:DefaultConnection").Value));

                    // Đăng ký GmailService
                    services.AddSingleton<GmailService>();

                    // Đăng ký HttpClient
                    services.AddHttpClient();

                    // Đăng ký SchedulerService
                    services.AddSingleton<SchedulerService>();

                    // Đăng ký Jobs
                    services.AddTransient<TransactionUpdateJobDB>();

                    // Đăng ký logging
                    services.AddLogging(logging => logging.AddConsole());
                });
    }

    public class Worker : BackgroundService
    {
        private readonly SchedulerService _schedulerService;

        public Worker(SchedulerService schedulerService)
        {
            _schedulerService = schedulerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _schedulerService.StartAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _schedulerService.StopAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}