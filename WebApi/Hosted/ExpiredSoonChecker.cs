
namespace WebApi.Hosted
{
    public class ExpiredSoonChecker(IServiceProvider services) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
