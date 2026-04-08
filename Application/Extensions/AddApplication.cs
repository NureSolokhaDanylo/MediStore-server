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
            .AddScoped(typeof(IReadOnlyService<>), typeof(ReadEntityService<>))
            .AddScoped(typeof(ICreateService<>), typeof(CreateEntityService<>))
            .AddScoped(typeof(IUpdateService<>), typeof(UpdateEntityService<>))
            .AddScoped(typeof(IDeleteService<>), typeof(DeleteEntityService<>))
            .AddScoped(typeof(IEntityAuditService<>), typeof(NullEntityAuditService<>))
            .AddScoped<IEntityAuditService<Zone>, ZoneAuditService>()
            .AddScoped<IEntityAuditService<Batch>, BatchAuditService>()
            .AddScoped<IEntityAuditService<Medicine>, MedicineAuditService>()
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
            .AddScoped<IReportService, ReportService>()
            .AddScoped<IUserDeviceService, UserDeviceService>();

        return services;
    }
}
