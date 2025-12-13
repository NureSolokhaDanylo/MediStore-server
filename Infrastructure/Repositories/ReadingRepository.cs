using Domain.Models;
using Domain.Enums;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReadingRepository : Repository<Reading>, IReadingRepository
    {
        public ReadingRepository(AppDbContext context) : base(context) { }

        public Task<List<Reading>> GetReadingsForZoneAsync(int zoneId, DateTime since, SensorType sensorType)
        {
            // join readings -> sensors -> zones, filter by zoneId and sensor type and timestamp
            return _context.Set<Reading>()
                .Where(r => r.TimeStamp >= since && r.Sensor.ZoneId == zoneId && r.Sensor.SensorType == sensorType)
                .ToListAsync();
        }

        public Task<List<Reading>> GetReadingsForSensorAsync(int sensorId, DateTime from, DateTime to)
        {
            return _context.Set<Reading>()
                .Where(r => r.SensorId == sensorId && r.TimeStamp >= from && r.TimeStamp <= to)
                .OrderBy(r => r.TimeStamp)
                .ToListAsync();
        }
    }
}
