using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.Shared.Data;
using System.Security.Claims;

namespace PersonalFinanceManager.API.Extensions
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser>
    {
        public CustomUserClaimsPrincipalFactory(UserManager<AppUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName ?? user.Email));
            return identity;
        }
    }
}
