using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Hosting = Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalFinanceManager.Scheduler.Jobs;
using PersonalFinanceManager.Scheduler.Models;
using PersonalFinanceManager.Scheduler.Services;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Services;

namespace PersonalFinanceManager.Scheduler.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Hosting.Host.CreateDefaultBuilder(args)
                .UseWindowsService() // Thêm để hỗ trợ Windows Service
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole(options => options.IncludeScopes = true);
                        logging.AddEventLog(); // Log vào Windows Event Log khi chạy như service
                        logging.SetMinimumLevel(LogLevel.Debug);
                    });

                    services.AddHostedService<Worker>();

                    services.Configure<SchedulerConfig>(hostContext.Configuration.GetSection("Scheduler"));

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(hostContext.Configuration.GetSection("Scheduler:DefaultConnection").Value));

                    services.AddSingleton<GmailService>();
                    services.AddHttpClient();
                    services.AddSingleton<SchedulerService>();
                    services.AddTransient<TransactionUpdateJobDB>();
                });
    }
}