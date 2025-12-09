using Application.Interfaces;
using Application.Services;

using Domain.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddScoped<IUserService, UserService>()
            .AddScoped<IZoneService, ZoneService>()
            .AddScoped<IBatchService, BatchService>()
            .AddScoped<IMedicineService, MedicineService>()
            .AddScoped<IAlertService, AlertService>()
            .AddScoped<ISensorService, SensorService>()
            .AddScoped<IReadingService, ReadingService>()
            .AddScoped<ISensorApiKeyService, SensorApiKeyService>()
            .AddScoped<IPasswordHasher<SensorApiKey>, PasswordHasher<SensorApiKey>>();

        return services;
    }
}
