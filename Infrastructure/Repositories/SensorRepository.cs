using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class SensorRepository : Repository<Sensor>, ISensorRepository
    {
        public SensorRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Sensor>> GetByZoneIdAsync(int zoneId)
        {
            return await _context.Set<Sensor>()
                .Where(s => s.ZoneId == zoneId)
                .OrderBy(s => s.SerialNumber)
                .ToListAsync();
        }
    }
}
