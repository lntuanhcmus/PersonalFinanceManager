﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Data.Entity
{
    public class TransactionCorrection
    {
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime TransactionTime { get; set; }

        public int TransactionTypeId { get; set; }

        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
