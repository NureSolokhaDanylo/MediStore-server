using Application.Extensions;

using Infrastructure;

using SharedConfiguration;
using SharedConfiguration.Options;

using WebApi.Extensions;
using WebApi.Hosted;
using QuestPDF.Infrastructure;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
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