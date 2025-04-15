using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Scheduler.Models
{
    public class GmailJobParameters : IJobParameters
    {
        public string CredentialsPath { get; set; }
        public int MaxResults { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
