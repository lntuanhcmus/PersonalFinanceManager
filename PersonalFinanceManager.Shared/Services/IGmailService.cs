using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using PersonalFinanceManager.Shared.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Services
{
    public interface IGmailService
    {
        public Task<List<Transaction>> ExtractTransactionsAsync(string credentialsPath, int maxResult = 10, UserCredential? credential = null);

        public Task<UserCredential> ExchangeCodeForTokenAsync(string code);
    }
}
