using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace PersonalFinanceManager.Shared.ML.Models
{
    public class TransactionPrediction
    {
        [ColumnName("PredictedTransactionType")]
        public string TransactionTypeId { get; set; }

        [ColumnName("PredictedCategory")]
        public string CategoryId { get; set; }

        public float[] Score { get; set; } // Độ tin cậy
    }
}
