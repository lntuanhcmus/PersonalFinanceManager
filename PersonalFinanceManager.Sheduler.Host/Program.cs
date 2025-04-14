using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Hosting = Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalFinanceManager.Scheduler.Jobs;
using PersonalFinanceManager.Scheduler.Models;
using PersonalFinanceManager.Scheduler.Services;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Services;
using Serilog;

namespace PersonalFinanceManager.Scheduler.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()  // Chỉ ghi các log từ Information trở lên
            .WriteTo.Console()           // Ghi log vào Console
            .WriteTo.File(
                "C:/PersonalFinanceLogs/scheduler-.log",  // Đường dẫn tới file log
                rollingInterval: RollingInterval.Day,    // Tạo file mới mỗi ngày
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information) // Ghi log từ Information trở lên
            .Filter.ByExcluding(logEvent =>
                logEvent.Level == Serilog.Events.LogEventLevel.Information &&
                logEvent.MessageTemplate.Text.Contains("Executing SqlQuery"))  // Loại bỏ câu query SQL
            .CreateLogger();

            try
            {
                Log.Information("Starting Scheduler.Host service...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Scheduler.Host terminated unexpectedly!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Hosting.Host.CreateDefaultBuilder(args)
                .UseWindowsService() // Thêm để hỗ trợ Windows Service
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.Configure<SchedulerConfig>(hostContext.Configuration.GetSection("Scheduler"));

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlServer(hostContext.Configuration.GetSection("Scheduler:DefaultConnection").Value)
                           .EnableSensitiveDataLogging(false)
                           .LogTo(_ => { }, LogLevel.None);
                    });

                    services.AddSingleton<GmailService>();
                    services.AddHttpClient();
                    services.AddSingleton<SchedulerService>();
                    services.AddTransient<TransactionUpdateJobDB>();
                    services.AddTransient<MLTrainingJob>();
                });
    }
}