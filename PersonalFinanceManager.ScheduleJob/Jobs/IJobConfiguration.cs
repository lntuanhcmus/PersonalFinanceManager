using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Scheduler.Jobs
{
    public interface IJobConfiguration
    {
        string JobName { get; }
        string CronSchedule { get; }
    }
}
