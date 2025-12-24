using System;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IDbExecutionStrategyProvider
    {
        Task ExecuteAsync(Func<Task> operation);
    }
}
