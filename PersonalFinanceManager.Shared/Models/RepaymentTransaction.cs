using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Models
{
    public class RepaymentTransaction
    {
        [Key]
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public Transaction Transaction { get; set; }

        public DateTime TransactionTime { get; set; }

        public DateTime CreatedAt { get; set; }

        public decimal Amount { get; set; }

        public string SenderName { get; set; }

        public string Description { get; set; }

    }
}
