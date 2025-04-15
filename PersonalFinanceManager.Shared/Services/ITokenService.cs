using Google.Apis.Auth.OAuth2;
using PersonalFinanceManager.Shared.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Services
{
    public interface ITokenService
    {
        Task SaveTokenAsync(ExternalToken token);

        Task<ExternalToken> GetTokenAsync(string provider, string userEmail = null);
    }
}
