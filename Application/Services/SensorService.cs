using Application.Interfaces;
using Application.Results.Base;

using Domain.Enums;
using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class SensorService : ReadOnlyService<Sensor>, ISensorService
{
    private readonly ISensorRepository _sensorRepository;
    
    public SensorService(ISensorRepository repository, IUnitOfWork uow) : base(repository, uow) 
    {
        _sensorRepository = repository;
    }

    public virtual async Task<Result<Sensor>> Add(Sensor entity)
    {
        if (entity.ZoneId is not null)
        {
            var zone = await _uow.Zones.GetAsync((int)entity.ZoneId);
            if (zone is null)
                return Result<Sensor>.Failure(new ErrorInfo
                {
                    Code = "sensor.zone_not_found",
                    Message = "Zone not found",
                    Type = ErrorType.NotFound,
                    Details = new Dictionary<string, object?> { ["zoneId"] = entity.ZoneId }
                });
        }

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync();   // <-- CRUD SaveChanges
        return Result<Sensor>.Success(entity);
    }

    public async Task<Result> Delete(int id)
    {
        var entity = await _repository.GetAsync(id);
        if (entity is null)
            return Result.Failure(new ErrorInfo
            {
                Code = "sensor.not_found",
                Message = "Not found",
                Type = ErrorType.NotFound,
                Details = new Dictionary<string, object?> { ["sensorId"] = id }
            });

        await _repository.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<Sensor>> UpdateFromAdmin(int id, string? serialNumber, bool? isOn, int? zoneId)
    {
        var existing = await _uow.Sensors.GetAsync(id);
        if (existing is null) return Result<Sensor>.Failure(new ErrorInfo
        {
            Code = "sensor.not_found",
            Message = "Not found",
            Type = ErrorType.NotFound,
            Details = new Dictionary<string, object?> { ["sensorId"] = id }
        });

        if (serialNumber is not null) existing.SerialNumber = serialNumber;
        if (isOn.HasValue) existing.IsOn = isOn.Value;

        if (existing.ZoneId != zoneId)
        {
            existing.LastValue = null;
            existing.LastUpdate = null;
        }

        existing.ZoneId = zoneId;

        _uow.Sensors.Update(existing);
        await _uow.SaveChangesAsync();

        return Result<Sensor>.Success(existing);
    }

    public async Task<Result<IEnumerable<Sensor>>> GetByZoneIdAsync(string userId, int zoneId)
    {
        try
        {
            var sensors = await _sensorRepository.GetByZoneIdAsync(zoneId);
            return Result<IEnumerable<Sensor>>.Success(sensors);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Sensor>>.Failure(new ErrorInfo
            {
                Code = "sensor.retrieval_failed",
                Message = $"Error retrieving sensors: {ex.Message}",
                Type = ErrorType.Unexpected
            });
        }
    }

    public async Task<Result<(IEnumerable<Sensor> Items, int TotalCount)>> GetPagedAsync(
        string userId,
        int skip,
        int take,
        string? q = null,
        SensorType? sensorType = null,
        bool? isOn = null,
        int? zoneId = null)
    {
        try
        {
            if (take <= 0)
            {
                return Result<(IEnumerable<Sensor> Items, int TotalCount)>.Failure(new ErrorInfo
                {
                    Code = "sensor.invalid_paging",
                    Message = "take must be greater than 0",
                    Type = ErrorType.Validation,
                    Details = new Dictionary<string, object?> { ["field"] = "take" }
                });
            }

            if (skip < 0)
            {
                return Result<(IEnumerable<Sensor> Items, int TotalCount)>.Failure(new ErrorInfo
                {
                    Code = "sensor.invalid_paging",
                    Message = "skip cannot be negative",
                    Type = ErrorType.Validation,
                    Details = new Dictionary<string, object?> { ["field"] = "skip" }
                });
            }

            var result = await _sensorRepository.GetPagedAsync(skip, take, q, sensorType, isOn, zoneId);
            return Result<(IEnumerable<Sensor> Items, int TotalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<(IEnumerable<Sensor> Items, int TotalCount)>.Failure(new ErrorInfo
            {
                Code = "sensor.retrieval_failed",
                Message = $"Error retrieving sensors: {ex.Message}",
                Type = ErrorType.Unexpected
            });
        }
    }
}
