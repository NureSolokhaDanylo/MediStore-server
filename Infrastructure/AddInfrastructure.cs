using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.UOW;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using SharedConfiguration.Options;

namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>((p, o) =>
            {
                var options = p.GetRequiredService<IOptions<InfrastructureOptions>>().Value;
                o.UseLazyLoadingProxies().UseSqlServer(options.ConnectionString);
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            //services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IZoneRepository, ZoneRepository>();
            services.AddScoped<IBatchRepository, BatchRepository>();
            services.AddScoped<IMedicineRepository, MedicineRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();
            services.AddScoped<ISensorRepository, SensorRepository>();
            services.AddScoped<IReadingRepository, ReadingRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
