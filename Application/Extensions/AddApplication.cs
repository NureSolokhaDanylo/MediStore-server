using Application.Interfaces;
using Application.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IZoneService, ZoneService>();
        services.AddScoped<IBatchService, BatchService>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<IReadingService, ReadingService>();
        return services;
    }
}
