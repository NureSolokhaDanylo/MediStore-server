using Domain.Models;
using Domain.Enums;
using Infrastructure.UOW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Hosted;

public class BatchConditionChecker(IServiceProvider services) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var logger = services.GetService<ILogger<BatchConditionChecker>>();
        var defaultDelay = TimeSpan.FromMinutes(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            AppSettings? appSettings = null;

            try
            {
                using var scope = services.CreateScope();
                var provider = scope.ServiceProvider;

                var uow = provider.GetRequiredService<IUnitOfWork>();

                appSettings = await uow.AppSettings.GetAsync();
                if (appSettings is null || !appSettings.AlertEnabled)
                {
                    await Task.Delay(defaultDelay, stoppingToken);
                    continue;
                }

                var interval = appSettings.CheckDeviationInterval;
                var since = DateTime.UtcNow - interval;

                var (batches, zones) = await LoadBatchesAndZonesAsync(uow);

                foreach (var batch in batches)
                {
                    try
                    {
                        await ProcessBatchAsync(uow, appSettings, since, batch, zones, logger);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Error processing batch {BatchId}", batch.Id);
                    }
                }

                // sleep using configured interval
                var sleep = appSettings.CheckDeviationInterval;
                await Task.Delay(sleep, stoppingToken);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error in BatchConditionChecker loop");
                await Task.Delay(defaultDelay, stoppingToken);
            }
        }
    }

    private static async Task<(List<Batch> Batches, Dictionary<int, Zone> Zones)> LoadBatchesAndZonesAsync(IUnitOfWork uow)
    {
        var batches = (await uow.Batches.GetAllAsync()).ToList();
        var zones = (await uow.Zones.GetAllWithSensorsAsync()).ToDictionary(z => z.Id);
        return (batches, zones);
    }

    private static async Task ProcessBatchAsync(IUnitOfWork uow, AppSettings appSettings, DateTime since, Batch batch, Dictionary<int, Zone> zones, ILogger? logger)
    {
        if (!zones.TryGetValue(batch.ZoneId, out var zone)) return;

        var med = await uow.Medicines.GetAsync(batch.MedicineId);
        if (med is null) return;

        var sensors = zone.Sensors.Where(s => s.LastUpdate.HasValue && s.LastUpdate.Value >= since && s.LastValue.HasValue).ToList();
        if (!sensors.Any()) return;

        var (shouldAlert, message) = EvaluateSensorsForBatch(appSettings, batch, med, sensors);

        if (!shouldAlert) return;

        var alert = new Alert
        {
            BatchId = batch.Id,
            ZoneId = batch.ZoneId,
            SensorId = null,
            AlertType = AlertType.BatchConditionWarning,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Message = message
        };

        await uow.Alerts.AddAsync(alert);
        await uow.SaveChangesAsync();

        logger?.LogInformation("Created batch condition alert for batch {BatchId}", batch.Id);
    }

    private static (bool ShouldAlert, string Message) EvaluateSensorsForBatch(AppSettings appSettings, Batch batch, Medicine med, List<Sensor> sensors)
    {
        bool shouldAlert = false;
        var sb = new System.Text.StringBuilder();
        sb.Append($"Batch {batch.Id}");
        if (!string.IsNullOrEmpty(batch.BatchNumber)) sb.Append($" (#{batch.BatchNumber})");
        sb.Append($" for Medicine: {med.Name}; ");

        foreach (var s in sensors)
        {
            var value = s.LastValue!.Value;

            if (s.SensorType == SensorType.Temperature)
            {
                var max = med.TempMax;
                var min = med.TempMin;
                double dev = 0;
                if (value > max) dev = value - max;
                else if (value < min) dev = min - value;
                else
                {
                    var devToMax = max - value;
                    var devToMin = value - min;
                    dev = Math.Min(devToMax, devToMin);
                }

                if (dev < appSettings.TempAlertDeviation)
                {
                    shouldAlert = true;
                    sb.Append($"Temp dev={dev:F2}, Last={value:F2} at {s.LastUpdate:yyyy-MM-dd HH:mm:ss} UTC (sensor {s.Id}); ");
                }
            }
            else if (s.SensorType == SensorType.Humidity)
            {
                var max = med.HumidMax;
                var min = med.HumidMin;
                double dev = 0;
                if (value > max) dev = value - max;
                else if (value < min) dev = min - value;
                else
                {
                    var devToMax = max - value;
                    var devToMin = value - min;
                    dev = Math.Min(devToMax, devToMin);
                }

                if (dev < appSettings.HumidityAlertDeviation)
                {
                    shouldAlert = true;
                    sb.Append($"Hum dev={dev:F2}, Last={value:F2} at {s.LastUpdate:yyyy-MM-dd HH:mm:ss} UTC (sensor {s.Id}); ");
                }
            }
        }

        return (shouldAlert, sb.ToString());
    }
}
