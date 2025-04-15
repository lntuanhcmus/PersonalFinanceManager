using Google.Apis.Auth.OAuth2;
using PersonalFinanceManager.Shared.Data;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public interface IGmailService
    {
        public Task<List<Transaction>> ExtractTransactionsAsync(string credentialsPath, int maxResult = 10, UserCredential? credential = null);

        public Task<UserCredential> ExchangeCodeForTokenAsync(string code);
    }
}
