using Domain.Models;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    public class SensorApiKeyRepository : Repository<SensorApiKey>, ISensorApiKeyRepository
    {
        public SensorApiKeyRepository(AppDbContext context) : base(context) { }
    }
}
