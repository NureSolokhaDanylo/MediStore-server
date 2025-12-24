using Domain.Models;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IReadingRepository : IRepository<Reading>
    {
        Task<List<Reading>> GetReadingsForZoneAsync(int zoneId, DateTime since, SensorType sensorType);
        Task<List<Reading>> GetReadingsForZoneAsync(int zoneId, DateTime from, DateTime to);
        Task<List<Reading>> GetLatestForZoneAsync(int zoneId, int count);
        Task<List<Reading>> GetReadingsForSensorAsync(int sensorId, DateTime from, DateTime to);
        Task<List<Reading>> GetLatestForSensorAsync(int sensorId, int count);
        Task<int> DeleteOlderThanAsync(DateTime cutoffUtc);
    }
}
