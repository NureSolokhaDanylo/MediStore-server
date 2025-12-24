using Application.Interfaces;
using Application.Services;
using Application.Seeders;

using Domain.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services
            .AddScoped<IAccountService, AccountService>()
            .AddScoped<IZoneService, ZoneService>()
            .AddScoped<IBatchService, BatchService>()
            .AddScoped<IMedicineService, MedicineService>()
            .AddScoped<IAlertService, AlertService>()
            .AddScoped<ISensorService, SensorService>()
            .AddScoped<IReadingService, ReadingService>()
            .AddScoped<ISensorApiKeyService, SensorApiKeyService>()
            .AddScoped<IAuditLogService, AuditLogService>()
            .AddScoped<IPasswordHasher<SensorApiKey>, PasswordHasher<SensorApiKey>>()
            .AddScoped<IAppSettingsService, AppSettingsService>()
            .AddScoped<IReportService, ReportService>();

        return services;
    }
}
