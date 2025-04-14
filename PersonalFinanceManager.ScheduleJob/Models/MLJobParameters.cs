using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Scheduler.Models
{
    public class MLJobParameters
    {
        public string CategoryModelPath { get; set; }
        public string TypeModelPath { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
