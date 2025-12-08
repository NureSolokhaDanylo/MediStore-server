using Application.Extensions;

using Infrastructure;

using SharedConfiguration;
using SharedConfiguration.Options;

using WebApi.Extensions;
using WebApi.Hosted;

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
                .AddHostedService<SeederHostedService>();

            #endregion

            #region Middleware

            var app = builder.Build();

            app.UseDeveloperFeatures();
            //no cors for now
            app.UseAppRequestPipeline(string.Empty);

            #endregion

            app.Run();
        }
    }
}