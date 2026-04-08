using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class ReadingService : IReadingService
{
    private readonly IReadOnlyService<Reading> _readService;
    private readonly IReadingRepository _readingRepo;
    private readonly IUnitOfWork _uow;

    public ReadingService(IReadOnlyService<Reading> readService, IReadingRepository repository, IUnitOfWork uow)
    {
        _readService = readService;
        _readingRepo = repository;
        _uow = uow;
    }

    public Task<Result<Reading>> Get(int id) => _readService.Get(id);

    public Task<Result<IEnumerable<Reading>>> GetAll() => _readService.GetAll();

    public async Task<Result<Reading>> CreateForSensorAsync(int sensorId, Reading reading)
    {
        // ensure sensor exists and is on
        var sensorRes = await _uow.Sensors.GetAsync(sensorId);
        if (sensorRes is null) return Result<Reading>.Failure(Errors.NotFound(ErrorCodes.Reading.SensorNotFound, "Sensor not found", "sensorId", sensorId));

        if (!sensorRes.IsOn) return Result<Reading>.Failure(Errors.Validation(
            ErrorCodes.Sensor.SensorOff,
            "Sensor is off",
            details: new Dictionary<string, object?> { ["sensorId"] = sensorId }));

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
        if (from >= to) return Result<IEnumerable<Reading>>.Failure(Errors.Validation(ErrorCodes.Reading.InvalidTimeRange, "Invalid time range"));

        var readings = await _readingRepo.GetReadingsForSensorAsync(sensorId, from, to);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetLatestReadingsForSensorAsync(int sensorId, int count)
    {
        if (count <= 0) return Result<IEnumerable<Reading>>.Failure(PagingErrors.InvalidCount(ErrorCodes.Reading.InvalidCount));
        var readings = await _readingRepo.GetLatestForSensorAsync(sensorId, count);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetReadingsForZoneAsync(int zoneId, DateTime from, DateTime to)
    {
        if (from >= to) return Result<IEnumerable<Reading>>.Failure(Errors.Validation(ErrorCodes.Reading.InvalidTimeRange, "Invalid time range"));
        var readings = await _readingRepo.GetReadingsForZoneAsync(zoneId, from, to);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetLatestReadingsForZoneAsync(int zoneId, int count)
    {
        if (count <= 0) return Result<IEnumerable<Reading>>.Failure(PagingErrors.InvalidCount(ErrorCodes.Reading.InvalidCount));
        var readings = await _readingRepo.GetLatestForZoneAsync(zoneId, count);
        return Result<IEnumerable<Reading>>.Success(readings);
    }
}
