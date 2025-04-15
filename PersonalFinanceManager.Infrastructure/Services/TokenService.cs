using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Data.Entity;
using PersonalFinanceManager.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _dbContext;

        public TokenService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ExternalToken> GetTokenAsync(string provider, string userEmail)
        {
            return await _dbContext.ExternalTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Provider == provider && t.UserEmail == userEmail);
        }

        public async Task SaveTokenAsync(ExternalToken token)
        {
            _dbContext.ExternalTokens.Add(token);
            await _dbContext.SaveChangesAsync();
        }
    }
}
