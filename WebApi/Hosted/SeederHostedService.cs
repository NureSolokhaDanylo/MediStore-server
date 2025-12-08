using Application.Seeders;

using Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebApi.Hosted
{
    public class SeederHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SeederHostedService> _logger;

        public SeederHostedService(IServiceProvider serviceProvider, ILogger<SeederHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var seeders = scope.ServiceProvider.GetServices<ISeeder>();

            if (!seeders.Any())
            {
                _logger.LogInformation("No seeders registered.");
                return;
            }

            foreach (var seeder in seeders)
            {
                try
                {
                    _logger.LogInformation("Running seeder {Seeder}", seeder.GetType().Name);
                    await seeder.SeedAsync();
                    _logger.LogInformation("[Seed] {Seeder} completed.", seeder.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Seed] {Seeder} skipped due to error", seeder.GetType().Name);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
