using Domain.Enums;
using Domain.Models;

namespace Infrastructure.Interfaces
{
    public interface ISensorRepository : IRepository<Sensor>
    {
        Task<IEnumerable<Sensor>> GetByZoneIdAsync(int zoneId);
        Task<(IEnumerable<Sensor> Items, int TotalCount)> GetPagedAsync(
            int skip,
            int take,
            string? q = null,
            SensorType? sensorType = null,
            bool? isOn = null,
            int? zoneId = null);
    }
}
