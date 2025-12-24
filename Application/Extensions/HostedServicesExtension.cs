using Application.Hosted;

using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    public static class HostedServicesExtension
    {
        public static IServiceCollection AddAppHostedServices(this IServiceCollection services)
        {
            services
                .AddHostedService<ExpiredChecker>()
                .AddHostedService<ExpirationSoonChecker>()
                .AddHostedService<BatchConditionChecker>()
                .AddHostedService<ZoneConditionChecker>()
                .AddHostedService<ReadingsRetentionCleaner>();

            return services;
        }
    }
}
