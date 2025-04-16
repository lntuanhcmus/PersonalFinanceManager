using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Shared.Data
{
    public class RefreshToken
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; } = false;

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = null!;
    }
}
