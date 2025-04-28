using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public interface IQueueService
    {
        Task SendMessageAsync<T>(T message);
    }
}
