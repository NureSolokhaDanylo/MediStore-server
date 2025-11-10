using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedConfiguration
{
    public static class AddConfiguration
    {
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration config)
        {
            //Пока эта регистрация не нужна, но мб для других сценариев сгодится
            //services.Configure<DataBaseOptions>(config.GetSection("DataBaseOptions"));

            return services;
        }
    }
}
