using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.Scheduler.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalFinanceManager.Scheduler.Models;
using Quartz.Spi;
using System.Text.Json;
using PersonalFinanceManager.Shared.Constants;

namespace PersonalFinanceManager.Scheduler.Services
{
    public class SchedulerService
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger<SchedulerService> _logger;
        private readonly SchedulerConfig _config;
        private readonly IServiceProvider _serviceProvider;

        public SchedulerService(
            IServiceProvider serviceProvider,
            ILogger<SchedulerService> logger,
            IOptions<SchedulerConfig> config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            var schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            _scheduler.JobFactory = new QuartzJobFactory(serviceProvider);
            _logger.LogDebug("Khởi tạo Quartz Scheduler với JobFactory.");
        }

        public async Task StartAsync()
        {
            _logger.LogDebug("Khởi động SchedulerService...");
            _logger.LogDebug("Cấu hình jobs: {Config}", JsonSerializer.Serialize(_config.Jobs));

            var jobTypes = new Dictionary<string, Type>
            {
                { BackgroundJobConstant.TransactionUpdateJobDb, typeof(TransactionUpdateJobDB) }
            };

            foreach (var jobConfig in _config.Jobs)
            {
                if (!jobTypes.ContainsKey(jobConfig.JobName))
                {
                    _logger.LogWarning($"Job {jobConfig.JobName} không được đăng ký.");
                    continue;
                }

                _logger.LogDebug("Tạo job {JobName} với cron {Cron}", jobConfig.JobName, jobConfig.CronSchedule);
                IJobDetail job = JobBuilder.Create(jobTypes[jobConfig.JobName])
                    .WithIdentity(jobConfig.JobName, "group1")
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jobConfig.JobName}Trigger", "group1")
                    .StartNow()
                    .WithCronSchedule(jobConfig.CronSchedule, x => x.WithMisfireHandlingInstructionFireAndProceed())
                    .Build();

                await _scheduler.ScheduleJob(job, trigger);
                _logger.LogInformation($"Đã lập lịch cho job {jobConfig.JobName} ({jobConfig.JobType}) với cron {jobConfig.CronSchedule}.");
            }

            await _scheduler.Start();
            _logger.LogDebug("SchedulerService đã khởi động.");

            // Trigger thủ công để test
            _logger.LogDebug("Trigger thủ công tất cả jobs...");
            foreach (var jobConfig in _config.Jobs)
            {
                await TriggerJobAsync(jobConfig.JobName);
            }
        }

        public async Task StopAsync()
        {
            _logger.LogDebug("Dừng SchedulerService...");
            await _scheduler.Shutdown();
            _logger.LogDebug("SchedulerService đã dừng.");
        }

        public async Task TriggerJobAsync(string jobName)
        {
            _logger.LogDebug("Trigger thủ công job {JobName}", jobName);
            var jobKey = new JobKey(jobName, "group1");
            if (await _scheduler.CheckExists(jobKey))
            {
                await _scheduler.TriggerJob(jobKey);
                _logger.LogDebug("Đã trigger job {JobName}", jobName);
            }
            else
            {
                _logger.LogWarning("Job {JobName} không tồn tại.", jobName);
            }
        }
    }

    public class QuartzJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public QuartzJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobType = bundle.JobDetail.JobType;
            var logger = _serviceProvider.GetService<ILogger<SchedulerService>>();
            logger?.LogDebug("Tạo job: {JobType}", jobType.Name);

            try
            {
                return _serviceProvider.GetRequiredService(jobType) as IJob;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Lỗi khi tạo job {JobType}", jobType.Name);
                throw;
            }
        }

        public void ReturnJob(IJob job)
        {
            // Không cần dispose job vì scope được quản lý trong ExecuteJob
        }
    }
}