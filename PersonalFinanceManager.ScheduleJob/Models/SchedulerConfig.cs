using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Scheduler.Models
{
    public class SchedulerConfig
    {
        public List<JobConfig> Jobs { get; set; }
        public string ConnectionString { get; set; } // Cho job DB
    }

    public class JobConfig
    {
        public string JobName { get; set; }
        public string JobType { get; set; } // "DB" hoặc "API"
        public string CronSchedule { get; set; }
        public string ParametersJson { get; set; } // JSON cho tham số đặc biệt
        public string ApiBaseUrl { get; set; } // Cho job API
    }
}
