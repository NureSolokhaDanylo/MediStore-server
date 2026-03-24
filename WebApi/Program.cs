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

            app.UseDeveloperFeatures();
            //no cors for now
            app.UseAppRequestPipeline(string.Empty);

            #endregion

            app.Run();
        }
    }
}