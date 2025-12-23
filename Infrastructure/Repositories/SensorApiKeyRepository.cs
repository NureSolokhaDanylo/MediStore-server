using Domain.Models;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    internal class SensorApiKeyRepository : Repository<SensorApiKey>, ISensorApiKeyRepository
    {
        public SensorApiKeyRepository(AppDbContext context) : base(context) { }
    }
}
