using System;
using Application.Seeders;
using Infrastructure.UOW;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Seeders
{
    public class AppSettingsSeeder : ISeeder
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AppSettingsSeeder> _logger;

        public AppSettingsSeeder(IUnitOfWork uow, ILogger<AppSettingsSeeder> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var existing = await _uow.AppSettings.GetAsync();
            if (existing is not null)
            {
                _logger.LogInformation("AppSettings already seeded.");
                return;
            }

            var settings = new AppSettings
            {
                AlertEnabled = true,
                TempAlertDeviation = 2.0,
                HumidityAlertDeviation = 5.0,
                CheckDeviationInterval = TimeSpan.FromMinutes(10),
                ReadingsRetentionDays = 30
            };

            await _uow.AppSettings.AddAsync(settings);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Seeded default AppSettings record.");
        }
    }
}
