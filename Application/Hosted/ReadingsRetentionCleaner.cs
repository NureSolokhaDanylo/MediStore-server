using Infrastructure.UOW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Hosted
{
    public class ReadingsRetentionCleaner(IServiceProvider services) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var logger = services.GetService<ILogger<ReadingsRetentionCleaner>>();
            var defaultDelay = TimeSpan.FromHours(1);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var provider = scope.ServiceProvider;
                    var uow = provider.GetRequiredService<IUnitOfWork>();

                    var settings = await uow.AppSettings.GetAsync();
                    if (settings is null)
                    {
                        await Task.Delay(defaultDelay, stoppingToken);
                        continue;
                    }

                    var cutoff = DateTime.UtcNow.AddDays(-settings.ReadingsRetentionDays);
                    var deleted = await uow.Readings.DeleteOlderThanAsync(cutoff);

                    if (deleted > 0)
                        logger?.LogInformation("Deleted {Count} readings older than {Cutoff}", deleted, cutoff);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error during readings retention cleanup");
                }

                try
                {
                    await Task.Delay(defaultDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
