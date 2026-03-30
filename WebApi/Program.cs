using Application.Extensions;
using Application.Seeders;

using Infrastructure;
using Infrastructure.Interfaces;

using QuestPDF.Infrastructure;

using SharedConfiguration;
using SharedConfiguration.Options;

using WebApi.Extensions;

namespace WebApi
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            var config = ConfigurationFactory.BuildConfiguration();

            // Configure URLs - always bind to all interfaces on port 5215
            // External hostname (api.medistore.app) is handled by Ingress/LoadBalancer
            builder.WebHost.UseUrls("http://+:5215");

            #region Building

            services
                .AddAppConfiguration(config)
                .AddInfrastructure()
                .AddAppIdentity()
                .AddApplication()
                .AddAppSeeders()
                .AddAuth()
                .AddAppControllersAndSwagger()
                .AddAppHostedServices();

            #endregion

            #region Middleware

            var app = builder.Build();
            await app.InitializeDatabaseAsync();

            using (var scope = app.Services.CreateScope())
            {
                var strategyProvider = scope.ServiceProvider.GetRequiredService<IDbExecutionStrategyProvider>();
                await strategyProvider.ExecuteAsync(async () =>
                {
                    foreach (var seeder in scope.ServiceProvider.GetServices<ISeeder>())
                        await seeder.SeedAsync();
                });
            }

            // Configure QuestPDF license to suppress runtime license validation exception during development
            QuestPDF.Settings.License = LicenseType.Community;

            var corsPolicy = app.Environment.IsDevelopment() ? "medistore-dev" : "medistore";

            app.UseDeveloperFeatures();
            app.UseAppRequestPipeline(corsPolicy);

            #endregion

            app.Run();
        }
    }
}
