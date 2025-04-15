using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace PersonalFinanceManager.Scheduler.Models
{
    public class TransactionPrediction
    {
        public string TransactionTypeId { get; set; }

        public string CategoryId { get; set; }
    }
}
