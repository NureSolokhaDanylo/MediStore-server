using Application.Seeders;

using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    public static class AddSeeders
    {
        public static IServiceCollection AddAppSeeders(this IServiceCollection services)
        {
            services.AddScoped<ISeeder, IdentitySeeder>()
                    .AddScoped<ISeeder, AppSettingsSeeder>();
            return services;
        }
    }
}
