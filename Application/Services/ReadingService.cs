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
        if (sensorRes is null) return Result<Reading>.Failure("Sensor not found");

        if (!sensorRes.IsOn) return Result<Reading>.Failure("Sensor is off");

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
        if (from >= to) return Result<IEnumerable<Reading>>.Failure("Invalid time range");

        var readings = await _readingRepo.GetReadingsForSensorAsync(sensorId, from, to);
        return Result<IEnumerable<Reading>>.Success(readings);
    }
}
