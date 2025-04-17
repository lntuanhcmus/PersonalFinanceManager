using Google.Apis.Auth.OAuth2;
using PersonalFinanceManager.Shared.Data;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public interface IGmailService
    {
        public Task<string> InitiateOAuthFlowAsync(string userId, string credentialsPath, string redirectUri);

        public Task<UserCredential> ExchangeCodeForTokenAsync(string userId, string credentialsPath, string code, string redirectUri);

        public Task<UserCredential> GetCredentialFromToken(string userId, string credentialsPath);
        
        public Task<List<Transaction>> ExtractTransactionsAsync(string userId, UserCredential userCredential, int maxResult = 10);

    }
}
