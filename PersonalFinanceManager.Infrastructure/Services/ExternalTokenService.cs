using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Infrastructure.Data;
using PersonalFinanceManager.Shared.Data;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public class ExternalTokenService : IExternalTokenService
    {
        private readonly AppDbContext _dbContext;

        public ExternalTokenService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ExternalToken> GetTokenAsync(string provider, string userEmail)
        {
            return await _dbContext.ExternalTokens
                .AsNoTracking()
                .Where(t => t.Provider == provider && t.UserEmail == userEmail)
                .OrderByDescending(t => t.IssuedUtc)
                .FirstOrDefaultAsync();
        }

        public async Task SaveTokenAsync(ExternalToken token)
        {
            _dbContext.ExternalTokens.Add(token);
            await _dbContext.SaveChangesAsync();
        }
    }
}
