using Infrastructure;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using SharedConfiguration.Options;

namespace Application.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddAppIdentity(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddOptions<IdentityOptions>()
                .PostConfigure<IOptions<AppIdentityOptions>>((o, appIdentity) =>
                {
                    var options = appIdentity.Value;

                    o.Password.RequiredLength = options.PasswordRequiredLength;
                    o.Password.RequireNonAlphanumeric = options.PasswordRequireNonAlphanumeric;
                    o.Password.RequireUppercase = options.PasswordRequireUppercase;
                    o.Password.RequireLowercase = options.PasswordRequireLowercase;
                    o.Password.RequireDigit = options.PasswordRequireDigit;

                    o.User.RequireUniqueEmail = options.UserRequireUniqueEmail;
                });

            return services;
        }
    }
}
