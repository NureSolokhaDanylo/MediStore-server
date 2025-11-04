using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace SharedConfiguration
{
    public static class ConfigurationFactory
    {
        public static IConfiguration BuildConfiguration(string? basePath = null)
        {
            var assemblyLocation = Path.GetDirectoryName(typeof(ConfigurationFactory).Assembly.Location)!;
            basePath ??= assemblyLocation;

            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("sharedsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
