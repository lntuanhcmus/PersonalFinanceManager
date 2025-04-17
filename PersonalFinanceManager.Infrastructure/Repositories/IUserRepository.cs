using PersonalFinanceManager.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<AppUser> FindByIdAsync(int userId);
        Task<AppUser> FindByEmailAsync(string email);

        Task SaveChangeAsync(AppUser user);
    }
}
