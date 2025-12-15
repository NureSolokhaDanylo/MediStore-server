using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApi.Hosted
{
    public class ExpirationSoonChecker(IServiceProvider services) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var logger = services.GetService<ILogger<ExpirationSoonChecker>>();

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

                    // determine maximum warning threshold to limit candidates
                    var medicines = await uow.Medicines.GetAllAsync();
                    var maxThreshold = medicines.Any() ? medicines.Max(m => m.WarningThresholdDays) : 0;
                    if (maxThreshold <= 0)
                    {
                        await Task.Delay(defaultDelay, stoppingToken);
                        continue;
                    }

                    var cutoff = DateTime.UtcNow.AddDays(maxThreshold);

                    var candidates = await LoadCandidateBatchesAsync(uow, cutoff);

                    foreach (var batch in candidates)
                    {
                        try
                        {
                            await ProcessCandidateBatchAsync(uow, appSettings, batch, cutoff, logger);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, "Error processing candidate batch {BatchId}", batch.Id);
                        }
                    }

                    // use configured interval for sleeping
                    var sleep = appSettings.CheckDeviationInterval;
                    await Task.Delay(sleep, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error in ExpirationSoonChecker loop");
                    await Task.Delay(defaultDelay, stoppingToken);
                }
            }
        }

        private static Task<List<Batch>> LoadCandidateBatchesAsync(IUnitOfWork uow, DateTime cutoff)
        {
            // previously used GetBatchesApproachingExpirationAsync(DateTime.UtcNow) then filtered by cutoff
            return uow.Batches.GetBatchesApproachingExpirationAsync(DateTime.UtcNow);
        }

        private static async Task ProcessCandidateBatchAsync(IUnitOfWork uow, AppSettings appSettings, Batch batch, DateTime cutoff, ILogger? logger)
        {
            // skip already expired (handled by ExpiredChecker) — we only want soon, not already expired
            if (batch.ExpireDate <= DateTime.UtcNow) return;

            // skip those beyond the global cutoff (they expire later than any medicine's threshold)
            if (batch.ExpireDate > cutoff) return;

            var med = await uow.Medicines.GetAsync(batch.MedicineId);
            if (med is null) return;

            // check per-batch threshold
            var thresholdDate = DateTime.UtcNow.AddDays(med.WarningThresholdDays);
            if (thresholdDate < batch.ExpireDate) return;

            // ensure no existing alert of this type for this batch
            var exists = await uow.Alerts.HasAlertForBatchAsync(batch.Id, Domain.Enums.AlertType.ExpirationSoon);
            if (exists) return;

            // build detailed message
            var medName = med.Name;
            var medDesc = med.Description ?? string.Empty;
            var medTempRange = $"Temp[{med.TempMin:F1}..{med.TempMax:F1}]";
            var medHumRange = $"Humid[{med.HumidMin:F1}..{med.HumidMax:F1}]";

            var daysLeft = (batch.ExpireDate - DateTime.UtcNow).TotalDays;

            var sb = new System.Text.StringBuilder();
            sb.Append($"Batch {batch.Id}");
            if (!string.IsNullOrEmpty(batch.BatchNumber)) sb.Append($" (#{batch.BatchNumber})");
            sb.Append($" for Medicine: {medName}");
            if (!string.IsNullOrEmpty(medDesc)) sb.Append($" - {medDesc}");
            sb.Append($"; Quantity: {batch.Quantity}; ");
            sb.Append($"Expires: {batch.ExpireDate.ToUniversalTime():yyyy-MM-dd HH:mm:ss} UTC; ");
            sb.Append($"DaysLeft: {daysLeft:F1}; ");
            sb.Append($"{medTempRange} {medHumRange}; ");
            sb.Append($"WarningThresholdDays: {med.WarningThresholdDays}");

            var alert = new Alert
            {
                BatchId = batch.Id,
                SensorId = null,
                ZoneId = null,
                AlertType = Domain.Enums.AlertType.ExpirationSoon,
                CreationTime = DateTime.UtcNow,
                Message = sb.ToString()
            };

            await uow.Alerts.AddAsync(alert);
            await uow.SaveChangesAsync();

            logger?.LogInformation("Created expiration-soon alert for batch {BatchId}", batch.Id);
        }
    }
}
