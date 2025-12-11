using Application.Interfaces;
using Application.Results.Base;
using Domain.Enums;
using Domain.Models;
using Infrastructure.UOW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApi.Hosted;

public class ZoneConditionChecker(IServiceProvider services) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var logger = services.GetService<ILogger<ZoneConditionChecker>>();

        var defaultDelay = TimeSpan.FromMinutes(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            AppSettings? appSettings = null;

            try
            {
                using var scope = services.CreateScope();
                var provider = scope.ServiceProvider;

                var uow = provider.GetRequiredService<IUnitOfWork>();

                appSettings = await uow.AppSettings.GetAsync(1);
                if (appSettings is null || !appSettings.AlertEnabled)
                {
                    await Task.Delay(defaultDelay, stoppingToken);
                    continue;
                }

                var interval = appSettings.CheckDeviationInterval;
                var since = DateTime.UtcNow - interval;

                // get zones with sensors
                var zones = await uow.Zones.GetAllWithSensorsAsync();

                foreach (var zone in zones)
                {
                    try
                    {
                        // collect latest values from sensors in the zone within the interval
                        var sensors = zone.Sensors.Where(s => s.LastUpdate.HasValue && s.LastUpdate.Value >= since && s.LastValue.HasValue).ToList();
                        if (!sensors.Any()) continue;

                        double? maxTempDev = null;
                        Sensor? maxTempSensor = null;

                        double? maxHumDev = null;
                        Sensor? maxHumSensor = null;

                        foreach (var s in sensors)
                        {
                            if (!s.LastValue.HasValue) continue;
                            var value = s.LastValue.Value;

                            if (s.SensorType == SensorType.Temperature)
                            {
                                var max = zone.TempMax;
                                var min = zone.TempMin;
                                double dev = 0;
                                if (value > max) dev = value - max;
                                else if (value < min) dev = min - value;
                                else
                                {
                                    var devToMax = max - value;
                                    var devToMin = value - min;
                                    dev = Math.Min(devToMax, devToMin);
                                }

                                if (maxTempDev is null || dev > maxTempDev) { maxTempDev = dev; maxTempSensor = s; }
                            }
                            else if (s.SensorType == SensorType.Humidity)
                            {
                                var max = zone.HumidMax;
                                var min = zone.HumidMin;
                                double dev = 0;
                                if (value > max) dev = value - max;
                                else if (value < min) dev = min - value;
                                else
                                {
                                    var devToMax = max - value;
                                    var devToMin = value - min;
                                    dev = Math.Min(devToMax, devToMin);
                                }

                                if (maxHumDev is null || dev > maxHumDev) { maxHumDev = dev; maxHumSensor = s; }
                            }
                        }

                        // compare with appSettings deviations: trigger if min distance to limit < threshold
                        bool shouldAlert = false;
                        var sb = new System.Text.StringBuilder();
                        sb.Append($"Zone {zone.Id} ({zone.Name}): ");

                        if (maxTempDev.HasValue)
                        {
                            sb.Append($"Temp dev={maxTempDev:F2}; ");
                            if (maxTempDev < appSettings.TempAlertDeviation) shouldAlert = true;
                            if (maxTempSensor is not null)
                                sb.Append($"LastTemp={maxTempSensor.LastValue:F2} at {maxTempSensor.LastUpdate:yyyy-MM-dd HH:mm:ss} UTC (sensor {maxTempSensor.Id}); ");
                        }

                        if (maxHumDev.HasValue)
                        {
                            sb.Append($"Hum dev={maxHumDev:F2}; ");
                            if (maxHumDev < appSettings.HumidityAlertDeviation) shouldAlert = true;
                            if (maxHumSensor is not null)
                                sb.Append($"LastHum={maxHumSensor.LastValue:F2} at {maxHumSensor.LastUpdate:yyyy-MM-dd HH:mm:ss} UTC (sensor {maxHumSensor.Id}); ");
                        }

                        if (shouldAlert)
                        {
                            int? sensorId = maxTempSensor?.Id ?? maxHumSensor?.Id;

                            // create alert (do not skip even if previous alerts exist)
                            var alertMsg = sb.ToString();
                            var alert = new Alert
                            {
                                ZoneId = zone.Id,
                                SensorId = sensorId,
                                AlertType = AlertType.ZoneConditionAlert,
                                CreationTime = DateTime.UtcNow,
                                Message = alertMsg
                            };

                            await uow.Alerts.AddAsync(alert);
                            await uow.SaveChangesAsync();

                            logger?.LogInformation("Created zone condition alert for zone {ZoneId}", zone.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Error processing zone {ZoneId}", zone.Id);
                    }
                }

                // use configured interval for sleeping
                var sleep = appSettings.CheckDeviationInterval;
                await Task.Delay(sleep, stoppingToken);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error in ZoneConditionChecker loop");
                await Task.Delay(defaultDelay, stoppingToken);
            }
        }
    }
}
