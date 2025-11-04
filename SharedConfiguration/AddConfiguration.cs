using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharedConfiguration.Options;

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
