using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Extensions;

public static class HostExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost app)
    {
        var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        var delay = TimeSpan.FromSeconds(5);
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await dbContext.Database.MigrateAsync();

                startupLogger.LogInformation("Database migrations applied successfully on attempt {Attempt}.", attempt);
                return;
            }
            catch (Exception ex)
            {
                startupLogger.LogWarning(
                    ex,
                    "Failed to create DbContext or apply migrations on attempt {Attempt}. Retrying in {DelaySeconds} seconds.",
                    attempt,
                    delay.TotalSeconds);
            }

            await Task.Delay(delay);
        }
    }
}
