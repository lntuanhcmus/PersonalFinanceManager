using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Data.Entity
{
    public class LabelingRule
    {
        public int Id { get; set; }

        public string Keyword { get; set; }

        public int TransactionTypeId { get; set; }

        public int CategoryId { get; set; }
    }
}
