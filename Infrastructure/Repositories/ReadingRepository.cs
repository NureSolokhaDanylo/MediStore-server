using Domain.Models;
using Domain.Enums;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class ReadingRepository : Repository<Reading>, IReadingRepository
    {
        public ReadingRepository(AppDbContext context) : base(context) { }

        public Task<List<Reading>> GetReadingsForZoneAsync(int zoneId, DateTime since, SensorType sensorType)
        {
            return _context.Set<Reading>()
                .Where(r => r.TimeStamp >= since && r.Sensor.ZoneId == zoneId && r.Sensor.SensorType == sensorType)
                .OrderBy(r => r.TimeStamp)
                .ToListAsync();
        }

        public Task<List<Reading>> GetReadingsForZoneAsync(int zoneId, DateTime from, DateTime to)
        {
            return _context.Set<Reading>()
                .Where(r => r.Sensor.ZoneId == zoneId && r.TimeStamp >= from && r.TimeStamp <= to)
                .OrderBy(r => r.TimeStamp)
                .ToListAsync();
        }

        public async Task<List<Reading>> GetLatestForZoneAsync(int zoneId, int count)
        {
            var list = await _context.Set<Reading>()
                .Where(r => r.Sensor.ZoneId == zoneId)
                .OrderByDescending(r => r.TimeStamp)
                .Take(count)
                .ToListAsync();

            return list.OrderBy(r => r.TimeStamp).ToList();
        }

        public Task<List<Reading>> GetReadingsForSensorAsync(int sensorId, DateTime from, DateTime to)
        {
            return _context.Set<Reading>()
                .Where(r => r.SensorId == sensorId && r.TimeStamp >= from && r.TimeStamp <= to)
                .OrderBy(r => r.TimeStamp)
                .ToListAsync();
        }

        public async Task<List<Reading>> GetLatestForSensorAsync(int sensorId, int count)
        {
            var list = await _context.Set<Reading>()
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.TimeStamp)
                .Take(count)
                .ToListAsync();

            return list.OrderBy(r => r.TimeStamp).ToList();
        }

        public Task<int> DeleteOlderThanAsync(DateTime cutoffUtc)
        {
            return _context.Set<Reading>()
                .Where(r => r.TimeStamp < cutoffUtc)
                .ExecuteDeleteAsync();
        }
    }
}
