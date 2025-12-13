using Microsoft.OpenApi.Models;
using WebApi.Services;

namespace WebApi.Extensions
{
    public static class ControllersExtension
    {
        public static IServiceCollection AddAppControllersAndSwagger(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MediStore-server", Version = "v1" });
                var jwtScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token. Example: Bearer {token}",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };
                c.AddSecurityDefinition("Bearer", jwtScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtScheme, Array.Empty<string>() }
                });
            });

            // register web-specific services
            services.AddScoped<IReportDocumentService, ReportDocumentService>();

            return services;
        }
    }
}
