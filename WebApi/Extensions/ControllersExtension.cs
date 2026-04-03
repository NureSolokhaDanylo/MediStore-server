using Microsoft.AspNetCore.Components.Infrastructure;
using WebApi.HealthChecks;
using WebApi.Services;

namespace WebApi.Extensions
{
    public static class ControllersExtension
    {
        public static IServiceCollection AddAppControllers(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);
            services.AddEndpointsApiExplorer();

            services.AddOpenApi("v1");

            services.AddCors(options =>
            {
                options.AddPolicy("medistore", builder =>
                {
                    builder
                        .WithOrigins("https://medistore.app", "http://medistore.app")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });

                options.AddPolicy("medistore-dev", builder =>
                {
                    builder
                        .SetIsOriginAllowed(origin =>
                        {
                            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            {
                                return false;
                            }

                            return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                                || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
                        })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // register web-specific services
            services.AddScoped<IReportDocumentService, ReportDocumentService>();

            return services;
        }
    }
}
