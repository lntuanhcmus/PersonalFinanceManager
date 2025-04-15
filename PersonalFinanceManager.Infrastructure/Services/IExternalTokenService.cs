using PersonalFinanceManager.Shared.Data;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public interface IExternalTokenService
    {
        Task SaveTokenAsync(ExternalToken token);

        Task<ExternalToken> GetTokenAsync(string provider, string userEmail = null);
    }
}
