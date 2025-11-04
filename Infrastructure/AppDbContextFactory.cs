using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var contextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            contextOptionsBuilder.UseSqlServer(options.ConnectionString);

            return new AppDbContext(contextOptionsBuilder.Options);
        }
    }
}