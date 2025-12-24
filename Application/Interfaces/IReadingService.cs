using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Application.Interfaces;

public interface IReadingService : IReadOnlyService<Reading>
{
    Task<Result<Reading>> CreateForSensorAsync(int sensorId, Reading reading);
    Task<Result<IEnumerable<Reading>>> GetReadingsForSensorAsync(int sensorId, DateTime from, DateTime to);
    Task<Result<IEnumerable<Reading>>> GetLatestReadingsForSensorAsync(int sensorId, int count);
    Task<Result<IEnumerable<Reading>>> GetReadingsForZoneAsync(int zoneId, DateTime from, DateTime to);
    Task<Result<IEnumerable<Reading>>> GetLatestReadingsForZoneAsync(int zoneId, int count);
}
