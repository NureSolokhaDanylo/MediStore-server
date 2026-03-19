using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using SharedConfiguration;
using SharedConfiguration.Options;
namespace Infrastructure
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var config = ConfigurationFactory.BuildConfiguration();

            var options = config.GetSection("InfrastructureOptions").Get<InfrastructureOptions>();
            
            var connectionString = options?.ConnectionString 
                ?? "Server=localhost;Database=DesignTimeDb;Trusted_Connection=True;TrustServerCertificate=True";

            var contextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            contextOptionsBuilder.UseLazyLoadingProxies().UseSqlServer(connectionString);

            return new AppDbContext(contextOptionsBuilder.Options);
        }
    }
}