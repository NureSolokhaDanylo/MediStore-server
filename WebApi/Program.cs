using Application.Extensions;

using Infrastructure;

using SharedConfiguration;
using SharedConfiguration.Options;

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

            services.AddAppConfiguration(config);

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddInfrastructure(config.GetSection("InfrastructureOptions").Get<InfrastructureOptions>());
            services.AddApplication();
            services.AddAppIdentity();

            #endregion

            #region Middleware

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            #endregion

            app.Run();
        }
    }
}