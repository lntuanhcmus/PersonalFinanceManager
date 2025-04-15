using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.Infrastructure.Data;
using PersonalFinanceManager.Scheduler.Helpers;
using PersonalFinanceManager.Scheduler.Models;
using PersonalFinanceManager.Shared.Constants;
using Quartz;
using System.Text.Json;

namespace PersonalFinanceManager.Scheduler.Jobs
{
    public class MLTrainingJob : BaseJob, IJobConfiguration
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SchedulerConfig _config;
        private readonly ILogger<MLTrainingJob> _logger;
        private readonly string _basePath;

        public MLTrainingJob(
            IServiceProvider serviceProvider,
            ILogger<MLTrainingJob> logger,
            IOptions<SchedulerConfig> config)
            : base(logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
            _basePath = AppContext.BaseDirectory;
            _logger.LogInformation("Khởi tạo MLTrainingJob với base path: {BasePath}", _basePath);
        }

        public string JobName => BackgroundJobConstant.MLTrainingJob;
        public string JobType => "ML";
        public string CronSchedule => _config.Jobs.FirstOrDefault(j => j.JobName == JobName)?.CronSchedule ?? "0 0 * * * ?";

        protected override async Task ExecuteJob(IJobExecutionContext context)
        {
            _logger.LogInformation("Bắt đầu thực thi {JobName} tại {ExecutionTime}", JobName, DateTime.Now);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var jobConfig = _config.Jobs.FirstOrDefault(j => j.JobName == JobName);
                _logger.LogInformation("Đọc cấu hình job: {JobConfig}", JsonSerializer.Serialize(jobConfig));

                var parameters = string.IsNullOrEmpty(jobConfig?.ParametersJson)
                    ? new MLJobParameters()
                    : JsonSerializer.Deserialize<MLJobParameters>(jobConfig.ParametersJson);
                _logger.LogInformation("Tham số job: {JobParameters}", JsonSerializer.Serialize(parameters));

                // Kiểm tra trạng thái Enable/Disable của job
                if (!parameters.IsEnabled)
                {
                    _logger.LogInformation("{JobName} is disabled (IsEnabled = false). Skipping execution.", JobName);
                    return;
                }

                // Kiểm tra kết nối với database
                _logger.LogInformation("Kiểm tra kết nối database...");
                var canConnect = await dbContext.Database.CanConnectAsync();
                _logger.LogInformation("Kết nối database: {ConnectionStatus}", canConnect ? "Thành công" : "Thất bại");

                if (!canConnect)
                {
                    _logger.LogError("Không thể kết nối database với connection string: {ConnectionString}", dbContext.Database.GetConnectionString());
                    throw new InvalidOperationException("Không thể kết nối database.");
                }

                // Đảm bảo thư mục đích cho mô hình tồn tại
                var categoryModelPath = Path.Combine(_basePath, parameters.CategoryModelPath ?? "Data/categoryModel.zip");
                var typeModelPath = Path.Combine(_basePath, parameters.TypeModelPath ?? "Data/typeModel.zip");

                var categoryModelDir = Path.GetDirectoryName(categoryModelPath);
                var typeModelDir = Path.GetDirectoryName(typeModelPath);

                // Tạo thư mục nếu chưa tồn tại
                if (!string.IsNullOrEmpty(categoryModelDir) && !Directory.Exists(categoryModelDir))
                {
                    _logger.LogInformation("Tạo thư mục cho category model tại: {CategoryModelDir}", categoryModelDir);
                    Directory.CreateDirectory(categoryModelDir);
                }
                if (!string.IsNullOrEmpty(typeModelDir) && !Directory.Exists(typeModelDir))
                {
                    _logger.LogInformation("Tạo thư mục cho type model tại: {TypeModelDir}", typeModelDir);
                    Directory.CreateDirectory(typeModelDir);
                }

                // Chạy huấn luyện mô hình
                _logger.LogInformation("Chạy huấn luyện mô hình ML...");

                ModelTrainer.UpdateModel(dbContext, categoryModelPath, typeModelPath);

                // Hoàn tất huấn luyện và lưu mô hình
                _logger.LogInformation("Huấn luyện hoàn tất! Đã lưu mô hình vào {CategoryPath} và {TypePath}",
                    categoryModelPath, typeModelPath);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có ngoại lệ
                _logger.LogError(ex, "Lỗi khi thực thi {JobName}: {ErrorMessage}", JobName, ex.Message);
                throw;
            }
        }
    }
}
