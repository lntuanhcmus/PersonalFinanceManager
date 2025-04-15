using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Dto;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public interface IUserTokenService
    {
        Task<TokenDto> GenerateTokenAsync(AppUser user);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}
