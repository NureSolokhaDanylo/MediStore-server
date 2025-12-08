using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Interfaces;
using Application.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    public static class JwtGeneratorExtension
    {
        public static IServiceCollection AddAppJwtGenerator(this IServiceCollection services)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            return services;
        }
    }
}
