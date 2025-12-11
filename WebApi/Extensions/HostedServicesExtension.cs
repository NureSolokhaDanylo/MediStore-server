using WebApi.Hosted;

namespace WebApi.Extensions
{
    public static class HostedServicesExtension
    {
        public static IServiceCollection AddAppHostedServices(this IServiceCollection services)
        {
            services
                .AddHostedService<SeederHostedService>()
                .AddHostedService<ExpiredChecker>()
                .AddHostedService<ExpirationSoonChecker>()
                .AddHostedService<BatchConditionChecker>()
                .AddHostedService<ZoneConditionChecker>();

            return services;
        }
    }
}
