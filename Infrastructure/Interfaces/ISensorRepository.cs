using Domain.Models;

namespace Infrastructure.Interfaces
{
    public interface ISensorRepository : IRepository<Sensor>
    {
        Task<IEnumerable<Sensor>> GetByZoneIdAsync(int zoneId);
    }
}
