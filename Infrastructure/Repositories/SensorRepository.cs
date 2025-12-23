using Domain.Models;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    internal class SensorRepository : Repository<Sensor>, ISensorRepository
    {
        public SensorRepository(AppDbContext context) : base(context) { }
    }
}
