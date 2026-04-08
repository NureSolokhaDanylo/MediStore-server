using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Application.Services;

public class ZoneService : IZoneService
{
    private readonly IReadOnlyService<Zone> _readService;
    private readonly ICreateService<Zone> _createService;
    private readonly IUpdateService<Zone> _updateService;
    private readonly IDeleteService<Zone> _deleteService;
    private readonly IZoneRepository _zoneRepository;
    private readonly IRepository<Zone> _repository;

    public ZoneService(
        IReadOnlyService<Zone> readService,
        ICreateService<Zone> createService,
        IUpdateService<Zone> updateService,
        IDeleteService<Zone> deleteService,
        IZoneRepository repository)
    {
        _readService = readService;
        _createService = createService;
        _updateService = updateService;
        _deleteService = deleteService;
        _zoneRepository = repository;
        _repository = repository;
    }

    private Result Validate(Zone z)
    {
        if (z.TempMin < -50 || z.TempMin > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.ValidationFailed, "TempMin must be between -50 and 100", "tempMin"));

        if (z.TempMax < -50 || z.TempMax > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.ValidationFailed, "TempMax must be between -50 and 100", "tempMax"));

        if (z.TempMin > z.TempMax)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.ValidationFailed, "TempMin cannot be greater than TempMax", "tempMin"));

        if (z.HumidMin < 0 || z.HumidMin > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.ValidationFailed, "HumidMin must be between 0 and 100", "humidMin"));

        if (z.HumidMax < 0 || z.HumidMax > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.ValidationFailed, "HumidMax must be between 0 and 100", "humidMax"));

        if (z.HumidMin > z.HumidMax)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.ValidationFailed, "HumidMin cannot be greater than HumidMax", "humidMin"));

        return Result.Success();
    }

    public async Task<Result<Zone>> Add(string userId, Zone entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.Error!);

        return await _createService.Add(userId, entity);
    }

    public async Task<Result<Zone>> Update(string userId, Zone entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Zone>.Failure(Errors.NotFound(ErrorCodes.Zone.NotFound, "Not found", "zoneId", entity.Id));

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.Error!);

        return await _updateService.Update(userId, entity);
    }

    public Task<Result<Zone>> Get(int id) => _readService.Get(id);

    public Task<Result<IEnumerable<Zone>>> GetAll() => _readService.GetAll();

    public Task<Result> Delete(string userId, int id) => _deleteService.Delete(userId, id);

    public async Task<Result<(IEnumerable<Zone> items, int totalCount)>> Search(string userId, string query, int limit, int offset)
    {
        if (limit <= 0)
            return Result<(IEnumerable<Zone>, int)>.Failure(PagingErrors.InvalidLimit(ErrorCodes.Zone.InvalidSearchPaging));

        if (offset < 0)
            return Result<(IEnumerable<Zone>, int)>.Failure(PagingErrors.InvalidOffset(ErrorCodes.Zone.InvalidSearchPaging));

        var (items, totalCount) = await _zoneRepository.SearchAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Zone>, int)>.Success((items, totalCount));
    }

}
