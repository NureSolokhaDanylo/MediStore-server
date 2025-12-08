using System.Security.Claims;
using System.Security.Claims;
using System.Text;
using System.Text;

using Application.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;

using SharedConfiguration.Options;

namespace WebApi.Extensions
{
    public static class AuthExtension
    {
        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services
                .AddAppJwtGenerator()
                .AddAppJwtBearer()
                .AddAuthorization();

            return services;
        }

        public static IServiceCollection AddAppJwtBearer(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .PostConfigure<IOptions<AppJwtOptions>>((jwtOptions, appJwt) =>
                {
                    var o = appJwt.Value;
                    var key = Encoding.UTF8.GetBytes(o.Key);

                    jwtOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        ClockSkew = TimeSpan.FromMinutes(2),
                        ValidIssuer = o.Issuer,
                        ValidAudience = o.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        NameClaimType = ClaimTypes.Name,
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            return services;
        }
    }
}
