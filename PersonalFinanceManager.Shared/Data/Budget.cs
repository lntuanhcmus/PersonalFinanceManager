using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Shared.Data
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; } // "Chi - Ăn uống", "Chi - Mua sắm", ...

        public Category Category { get; set; } = null!;

        public decimal Amount { get; set; } // Số tiền tối đa cho ngân sách

        public string Period { get; set; } = string.Empty; // "Monthly", "Yearly" (chu kỳ ngân sách)

        public DateTime StartDate { get; set; } // Ngày bắt đầu ngân sách

        public DateTime? EndDate { get; set; } // Ngày kết thúc (null nếu không giới hạn)

        public int? UserId { get; set; }

        public AppUser User { get; set; } = null!;
    }
}
