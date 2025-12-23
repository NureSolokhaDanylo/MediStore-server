using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.UOW;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using SharedConfiguration.Options;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((p, o) =>
        {
            var options = p.GetRequiredService<IOptions<InfrastructureOptions>>().Value;
            o.UseLazyLoadingProxies()
            .UseSqlServer(
                options.ConnectionString,
                sql =>
                {
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    sql.CommandTimeout(120);
                });
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IZoneRepository, ZoneRepository>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<IReadingRepository, ReadingRepository>();
        services.AddScoped<ISensorApiKeyRepository, SensorApiKeyRepository>();
        services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();

        services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(
            sp.GetRequiredService<AppDbContext>(),
            sp.GetRequiredService<IMedicineRepository>(),
            sp.GetRequiredService<ISensorRepository>(),
            sp.GetRequiredService<IBatchRepository>(),
            sp.GetRequiredService<IReadingRepository>(),
            sp.GetRequiredService<IZoneRepository>(),
            sp.GetRequiredService<IAlertRepository>(),
            sp.GetRequiredService<ISensorApiKeyRepository>(),
            sp.GetRequiredService<IAppSettingsRepository>()));

        return services;
    }
}
