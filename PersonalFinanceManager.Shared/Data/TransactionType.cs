using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Shared.Data
{
    public class TransactionType
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
    }
}
