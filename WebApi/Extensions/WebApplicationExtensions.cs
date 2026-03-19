using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Extensions;

public static class HostExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost app)
    {
        var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var connectionString = dbContext.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Database connection string is not configured.");

            var csb = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            var databaseName = csb.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("Initial catalog is not configured in the connection string.");
            }
            csb.InitialCatalog = "master";

            const int maxAttempts = 12;
            var delay = TimeSpan.FromSeconds(5);

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await using var connection = new Microsoft.Data.SqlClient.SqlConnection(csb.ConnectionString);
                    await connection.OpenAsync();

                    await using var command = connection.CreateCommand();
                    command.CommandText = """
                        SELECT state_desc
                        FROM sys.databases
                        WHERE name = @databaseName
                        """;
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@databaseName", System.Data.SqlDbType.NVarChar, 128) { Value = databaseName });

                    var state = (string?)await command.ExecuteScalarAsync();

                    if (state is null || state == "ONLINE")
                    {
                        break;
                    }

                    startupLogger.LogWarning(
                        "Database {DatabaseName} is in state {State}. Waiting {DelaySeconds} seconds before retry.",
                        databaseName,
                        state,
                        delay.TotalSeconds);
                }
                catch (Microsoft.Data.SqlClient.SqlException ex)
                {
                    if (attempt == maxAttempts)
                    {
                        throw;
                    }

                    startupLogger.LogWarning(
                        ex,
                        "Database readiness check failed on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds} seconds.",
                        attempt,
                        maxAttempts,
                        delay.TotalSeconds);
                }

                if (attempt == maxAttempts)
                {
                    throw new InvalidOperationException($"Database {databaseName} did not become ready in time.");
                }

                await Task.Delay(delay);
            }

            await dbContext.Database.MigrateAsync();
        }
    }
}
