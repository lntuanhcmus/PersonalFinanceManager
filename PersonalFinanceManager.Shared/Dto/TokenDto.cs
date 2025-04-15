using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Dto
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }

        public string RefreshToken { get; set; } = string.Empty;
    }
}
