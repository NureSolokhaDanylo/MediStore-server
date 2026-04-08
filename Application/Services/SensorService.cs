using Application.Interfaces;
using Application.Results.Base;

using Domain.Enums;
using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class SensorService : ISensorService
{
    private readonly IReadOnlyService<Sensor> _readService;
    private readonly ISensorRepository _sensorRepository;
    private readonly IRepository<Sensor> _repository;
    private readonly IUnitOfWork _uow;
    
    public SensorService(IReadOnlyService<Sensor> readService, ISensorRepository repository, IUnitOfWork uow)
    {
        _readService = readService;
        _sensorRepository = repository;
        _repository = repository;
        _uow = uow;
    }

    public Task<Result<Sensor>> Get(int id) => _readService.Get(id);

    public Task<Result<IEnumerable<Sensor>>> GetAll() => _readService.GetAll();

    public virtual async Task<Result<Sensor>> Add(Sensor entity)
    {
        if (entity.ZoneId is not null)
        {
            var zone = await _uow.Zones.GetAsync((int)entity.ZoneId);
            if (zone is null)
                return Result<Sensor>.Failure(Errors.NotFound(ErrorCodes.Sensor.ZoneNotFound, "Zone not found", "zoneId", entity.ZoneId));
        }

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync();   // <-- CRUD SaveChanges
        return Result<Sensor>.Success(entity);
    }

    public async Task<Result> Delete(int id)
    {
        var entity = await _repository.GetAsync(id);
        if (entity is null)
            return Result.Failure(Errors.NotFound(ErrorCodes.Sensor.NotFound, "Not found", "sensorId", id));

        await _repository.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<Sensor>> UpdateFromAdmin(int id, string? serialNumber, bool? isOn, int? zoneId)
    {
        var existing = await _uow.Sensors.GetAsync(id);
        if (existing is null) return Result<Sensor>.Failure(Errors.NotFound(ErrorCodes.Sensor.NotFound, "Not found", "sensorId", id));

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
            return Result<IEnumerable<Sensor>>.Failure(Errors.Unexpected(ErrorCodes.Sensor.RetrievalFailed, $"Error retrieving sensors: {ex.Message}"));
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
                return Result<(IEnumerable<Sensor> Items, int TotalCount)>.Failure(PagingErrors.InvalidTake(ErrorCodes.Sensor.InvalidPaging, "take must be greater than 0"));
            }

            if (skip < 0)
            {
                return Result<(IEnumerable<Sensor> Items, int TotalCount)>.Failure(PagingErrors.InvalidSkip(ErrorCodes.Sensor.InvalidPaging));
            }

            var result = await _sensorRepository.GetPagedAsync(skip, take, q, sensorType, isOn, zoneId);
            return Result<(IEnumerable<Sensor> Items, int TotalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<(IEnumerable<Sensor> Items, int TotalCount)>.Failure(Errors.Unexpected(ErrorCodes.Sensor.RetrievalFailed, $"Error retrieving sensors: {ex.Message}"));
        }
    }
}
