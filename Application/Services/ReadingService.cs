using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class ReadingService : ReadOnlyService<Reading>, IReadingService
{
    private readonly IReadingRepository _readingRepo;

    public ReadingService(IReadingRepository repository, IUnitOfWork uow) : base(repository, uow) { _readingRepo = repository; }

    public async Task<Result<Reading>> CreateForSensorAsync(int sensorId, Reading reading)
    {
        // ensure sensor exists and is on
        var sensorRes = await _uow.Sensors.GetAsync(sensorId);
        if (sensorRes is null) return Result<Reading>.Failure(new ErrorInfo
        {
            Code = "reading.sensor_not_found",
            Message = "Sensor not found",
            Type = ErrorType.NotFound,
            Details = new Dictionary<string, object?> { ["sensorId"] = sensorId }
        });

        if (!sensorRes.IsOn) return Result<Reading>.Failure(new ErrorInfo
        {
            Code = "sensor.sensor_off",
            Message = "Sensor is off",
            Type = ErrorType.Validation,
            Details = new Dictionary<string, object?> { ["sensorId"] = sensorId }
        });

        // set sensor id
        reading.SensorId = sensorId;

        await _uow.Readings.AddAsync(reading);
        await _uow.SaveChangesAsync();

        // update sensor last value and update time
        sensorRes.LastValue = reading.Value;
        sensorRes.LastUpdate = DateTime.UtcNow;
        _uow.Sensors.Update(sensorRes);
        await _uow.SaveChangesAsync();

        return Result<Reading>.Success(reading);
    }

    public async Task<Result<IEnumerable<Reading>>> GetReadingsForSensorAsync(int sensorId, DateTime from, DateTime to)
    {
        if (from >= to) return Result<IEnumerable<Reading>>.Failure(new ErrorInfo
        {
            Code = "reading.invalid_time_range",
            Message = "Invalid time range",
            Type = ErrorType.Validation
        });

        var readings = await _readingRepo.GetReadingsForSensorAsync(sensorId, from, to);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetLatestReadingsForSensorAsync(int sensorId, int count)
    {
        if (count <= 0) return Result<IEnumerable<Reading>>.Failure(new ErrorInfo
        {
            Code = "reading.invalid_count",
            Message = "Count must be positive",
            Type = ErrorType.Validation,
            Details = new Dictionary<string, object?> { ["field"] = "count" }
        });
        var readings = await _readingRepo.GetLatestForSensorAsync(sensorId, count);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetReadingsForZoneAsync(int zoneId, DateTime from, DateTime to)
    {
        if (from >= to) return Result<IEnumerable<Reading>>.Failure(new ErrorInfo
        {
            Code = "reading.invalid_time_range",
            Message = "Invalid time range",
            Type = ErrorType.Validation
        });
        var readings = await _readingRepo.GetReadingsForZoneAsync(zoneId, from, to);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetLatestReadingsForZoneAsync(int zoneId, int count)
    {
        if (count <= 0) return Result<IEnumerable<Reading>>.Failure(new ErrorInfo
        {
            Code = "reading.invalid_count",
            Message = "Count must be positive",
            Type = ErrorType.Validation,
            Details = new Dictionary<string, object?> { ["field"] = "count" }
        });
        var readings = await _readingRepo.GetLatestForZoneAsync(zoneId, count);
        return Result<IEnumerable<Reading>>.Success(readings);
    }
}
