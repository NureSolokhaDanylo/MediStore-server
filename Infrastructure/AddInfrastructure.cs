using Infrastructure.Interfaces;
using Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SharedConfiguration.Options;

namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, InfrastructureOptions options)
        {
            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(options.ConnectionString));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IZoneRepository, ZoneRepository>();
            services.AddScoped<IBatchRepository, BatchRepository>();
            services.AddScoped<IMedicineRepository, MedicineRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();
            services.AddScoped<ISensorRepository, SensorRepository>();
            services.AddScoped<IReadingRepository, ReadingRepository>();

            return services;
        }
    }
}
