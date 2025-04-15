using Google.Apis.Auth.OAuth2.Responses;
using PersonalFinanceManager.Shared.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.Helpers
{
    public static class ExternalTokenExtensions
    {
        public static ExternalToken ToExternalToken(this TokenResponse token, string provider, string userEmail = null)
        {
            return new ExternalToken
            {
                UserEmail = userEmail,
                AccessToken = token.AccessToken,
                TokenType = token.TokenType,
                RefreshToken = token.RefreshToken ?? "",
                ExpiresInSeconds = token.ExpiresInSeconds,
                Scope = token.Scope,
                IdToken = token.IdToken,
                Issued = token.Issued,
                IssuedUtc = token.IssuedUtc,
                IsStale = token.IsStale,
                ExpiresAtUtc = token.IssuedUtc.AddSeconds(token.ExpiresInSeconds ?? 3600),
                Provider = provider,
            };
        }

        public static TokenResponse ToTokenResponse(this ExternalToken token)
        {
            return new TokenResponse
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType,
                RefreshToken = token.RefreshToken,
                ExpiresInSeconds = token.ExpiresInSeconds,
                Scope = token.Scope,
                IdToken = token.IdToken,
                Issued = token.Issued,
                IssuedUtc = token.IssuedUtc
            };
        }
    }
}
