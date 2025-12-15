using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApi.Hosted
{
    public class ExpiredChecker(IServiceProvider services) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var logger = services.GetService<ILogger<ExpiredChecker>>();

            var defaultDelay = TimeSpan.FromMinutes(1);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var provider = scope.ServiceProvider;

                    var uow = provider.GetRequiredService<IUnitOfWork>();

                    // check app settings: if alerts disabled skip
                    var appSettings = await uow.AppSettings.GetAsync(1);
                    if (appSettings is null || !appSettings.AlertEnabled)
                    {
                        await Task.Delay(defaultDelay, stoppingToken);
                        continue;
                    }

                    var batches = await LoadExpiredBatchesAsync(uow);

                    foreach (var batch in batches)
                    {
                        try
                        {
                            await ProcessExpiredBatchAsync(uow, batch, logger);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, "Error processing batch {BatchId}", batch.Id);
                        }
                    }

                    // use configured interval for sleeping
                    var sleep = appSettings.CheckDeviationInterval;
                    await Task.Delay(sleep, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error in ExpiredChecker loop");
                    await Task.Delay(defaultDelay, stoppingToken);
                }
            }
        }

        private static Task<List<Batch>> LoadExpiredBatchesAsync(IUnitOfWork uow)
        {
            return uow.Batches.GetExpiredBatchesAsync(DateTime.UtcNow);
        }

        private static async Task ProcessExpiredBatchAsync(IUnitOfWork uow, Batch batch, ILogger? logger)
        {
            // check existing alert using optimized repo method
            var exists = await uow.Alerts.HasAlertForBatchAsync(batch.Id, Domain.Enums.AlertType.Expired);
            if (exists) return;

            // create new alert
            var med = await uow.Medicines.GetAsync(batch.MedicineId);

            // build detailed info snapshot for the message (capture data at time of alert creation)
            var medName = med?.Name ?? "<unknown>";
            var medDesc = med?.Description ?? string.Empty;
            var medTempRange = med is null ? string.Empty : $"Temp[{med.TempMin:F1}..{med.TempMax:F1}]";
            var medHumRange = med is null ? string.Empty : $"Humid[{med.HumidMin:F1}..{med.HumidMax:F1}]";

            var message = new System.Text.StringBuilder();
            message.Append($"Batch {batch.Id}");
            if (!string.IsNullOrEmpty(batch.BatchNumber)) message.Append($" (#{batch.BatchNumber})");
            message.Append($" for Medicine: {medName}");
            if (!string.IsNullOrEmpty(medDesc)) message.Append($" - {medDesc}");
            if (!string.IsNullOrEmpty(medTempRange) || !string.IsNullOrEmpty(medHumRange))
                message.Append($" [{medTempRange} {medHumRange}]");

            message.Append($"; Quantity: {batch.Quantity}");
            message.Append($"; DateAdded: {batch.DateAdded.ToUniversalTime():yyyy-MM-dd HH:mm:ss} UTC");
            message.Append($"; ExpireDate: {batch.ExpireDate.ToUniversalTime():yyyy-MM-dd HH:mm:ss} UTC");

            var alert = new Alert
            {
                BatchId = batch.Id,
                SensorId = null,
                ZoneId = null,
                AlertType = Domain.Enums.AlertType.Expired,
                CreationTime = DateTime.UtcNow,
                Message = message.ToString()
            };

            await uow.Alerts.AddAsync(alert);
            await uow.SaveChangesAsync();

            logger?.LogInformation("Created expired alert for batch {BatchId}", batch.Id);
        }
    }
}
