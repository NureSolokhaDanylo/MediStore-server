using Domain.Enums;
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

        public async Task<(IEnumerable<Sensor> Items, int TotalCount)> GetPagedAsync(
            int skip,
            int take,
            string? q = null,
            SensorType? sensorType = null,
            bool? isOn = null,
            int? zoneId = null)
        {
            var query = _context.Set<Sensor>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var search = q.Trim();
                query = query.Where(s => s.SerialNumber.Contains(search));
            }

            if (sensorType.HasValue)
            {
                query = query.Where(s => s.SensorType == sensorType.Value);
            }

            if (isOn.HasValue)
            {
                query = query.Where(s => s.IsOn == isOn.Value);
            }

            if (zoneId.HasValue)
            {
                query = query.Where(s => s.ZoneId == zoneId.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.SerialNumber)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
