using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.UOW;

namespace Application.Services;

public class ReadingService : IReadingService
{
    private static readonly string[] ReadRoles = ["Admin", "Operator", "Observer"];
    private readonly IReadOnlyService<Reading> _readService;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentSensor _currentSensor;
    private readonly IAccessChecker _accessChecker;

    public ReadingService(IReadOnlyService<Reading> readService, IUnitOfWork uow, ICurrentSensor currentSensor, IAccessChecker accessChecker)
    {
        _readService = readService;
        _uow = uow;
        _currentSensor = currentSensor;
        _accessChecker = accessChecker;
    }

    public async Task<Result<Reading>> Get(int id)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<Reading>.Failure(access.Error!);

        return await _readService.Get(id);
    }

    public async Task<Result<IEnumerable<Reading>>> GetAll()
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<IEnumerable<Reading>>.Failure(access.Error!);

        return await _readService.GetAll();
    }

    public async Task<Result<Reading>> CreateForSensorAsync(Reading reading)
    {
        var sensorId = _currentSensor.SensorId;
        if (!_currentSensor.IsAuthenticated || !sensorId.HasValue)
        {
            return Result<Reading>.Failure(Errors.Unauthorized(ErrorCodes.SensorApiKey.InvalidKey, "Sensor is not authenticated"));
        }

        // ensure sensor exists and is on
        var sensorRes = await _uow.Sensors.GetAsync(sensorId.Value);
        if (sensorRes is null) return Result<Reading>.Failure(Errors.NotFound(ErrorCodes.Reading.SensorNotFound, "Sensor not found", "sensorId", sensorId.Value));

        if (!sensorRes.IsOn) return Result<Reading>.Failure(Errors.Validation(
            ErrorCodes.Sensor.SensorOff,
            "Sensor is off",
            details: new Dictionary<string, object?> { ["sensorId"] = sensorId.Value }));

        // set sensor id
        reading.SensorId = sensorId.Value;

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
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<IEnumerable<Reading>>.Failure(access.Error!);

        if (from >= to) return Result<IEnumerable<Reading>>.Failure(Errors.Validation(ErrorCodes.Reading.InvalidTimeRange, "Invalid time range"));

        var readings = await _uow.Readings.GetReadingsForSensorAsync(sensorId, from, to);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetLatestReadingsForSensorAsync(int sensorId, int count)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<IEnumerable<Reading>>.Failure(access.Error!);

        if (count <= 0) return Result<IEnumerable<Reading>>.Failure(PagingErrors.InvalidCount(ErrorCodes.Reading.InvalidCount));
        var readings = await _uow.Readings.GetLatestForSensorAsync(sensorId, count);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetReadingsForZoneAsync(int zoneId, DateTime from, DateTime to)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<IEnumerable<Reading>>.Failure(access.Error!);

        if (from >= to) return Result<IEnumerable<Reading>>.Failure(Errors.Validation(ErrorCodes.Reading.InvalidTimeRange, "Invalid time range"));
        var readings = await _uow.Readings.GetReadingsForZoneAsync(zoneId, from, to);
        return Result<IEnumerable<Reading>>.Success(readings);
    }

    public async Task<Result<IEnumerable<Reading>>> GetLatestReadingsForZoneAsync(int zoneId, int count)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<IEnumerable<Reading>>.Failure(access.Error!);

        if (count <= 0) return Result<IEnumerable<Reading>>.Failure(PagingErrors.InvalidCount(ErrorCodes.Reading.InvalidCount));
        var readings = await _uow.Readings.GetLatestForZoneAsync(zoneId, count);
        return Result<IEnumerable<Reading>>.Success(readings);
    }
}
