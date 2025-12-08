using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharedConfiguration.Options;

namespace SharedConfiguration
{
    public static class AddConfiguration
    {
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services
                .Configure<InfrastructureOptions>(config.GetSection("InfrastructureOptions"))
                .Configure<AppIdentityOptions>(config.GetSection("IdentityOptions"))
                .Configure<AppJwtOptions>(config.GetSection("JwtOptions"))
                .Configure<AppSeedOptions>(config.GetSection("SeedOptions"));


            return services;
        }
    }
}
