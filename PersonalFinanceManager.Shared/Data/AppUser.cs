using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PersonalFinanceManager.Shared.Data
{
    public class AppUser : IdentityUser<int>
    {
        // Thuộc tính cơ bản bổ sung
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        // Thuộc tính liên quan đến bảo mật
        public bool TwoFactorEnabled { get; set; } = false;

        public DateTime? LastLogin { get; set; }

        // Thuộc tính liên quan đến tài chính
        [MaxLength(3)]
        public string PreferredCurrency { get; set; } = "VND"; // Ví dụ: USD, VND

        // Thuộc tính để quản lý Gmail API
        public string? GmailRefreshToken { get; set; } // Lưu refresh token của Gmail API

        // Thuộc tính trạng thái tài khoản
        public bool IsActive { get; set; } = true;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        public bool IsConnectedGmail { get; set; } = false;
    }
}
