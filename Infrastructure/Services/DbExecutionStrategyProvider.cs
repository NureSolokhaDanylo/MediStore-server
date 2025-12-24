using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    internal class DbExecutionStrategyProvider : IDbExecutionStrategyProvider
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public DbExecutionStrategyProvider(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            await using var context = _contextFactory.CreateDbContext();
            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await operation();
            });
        }
    }
}
