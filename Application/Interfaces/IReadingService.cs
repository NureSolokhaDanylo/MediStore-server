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
}
