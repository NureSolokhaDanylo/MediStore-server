using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class ZoneService : IZoneService
{
    private static readonly string[] ReadRoles = ["Admin", "Operator", "Observer"];
    private readonly IReadOnlyService<Zone> _readService;
    private readonly ICreateService<Zone> _createService;
    private readonly IUpdateService<Zone> _updateService;
    private readonly IDeleteService<Zone> _deleteService;
    private readonly IUnitOfWork _uow;
    private readonly IAccessChecker _accessChecker;

    public ZoneService(
        IReadOnlyService<Zone> readService,
        ICreateService<Zone> createService,
        IUpdateService<Zone> updateService,
        IDeleteService<Zone> deleteService,
        IUnitOfWork uow,
        IAccessChecker accessChecker)
    {
        _readService = readService;
        _createService = createService;
        _updateService = updateService;
        _deleteService = deleteService;
        _uow = uow;
        _accessChecker = accessChecker;
    }

    private Result Validate(Zone z)
    {
        if (z.TempMin < -50 || z.TempMin > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.TempMinOutOfRange, "TempMin must be between -50 and 100", "tempMin"));

        if (z.TempMax < -50 || z.TempMax > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.TempMaxOutOfRange, "TempMax must be between -50 and 100", "tempMax"));

        if (z.TempMin > z.TempMax)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.TempRangeInvalid, "TempMin cannot be greater than TempMax", "tempMin"));

        if (z.HumidMin < 0 || z.HumidMin > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.HumidMinOutOfRange, "HumidMin must be between 0 and 100", "humidMin"));

        if (z.HumidMax < 0 || z.HumidMax > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.HumidMaxOutOfRange, "HumidMax must be between 0 and 100", "humidMax"));

        if (z.HumidMin > z.HumidMax)
            return Result.Failure(Errors.Validation(ErrorCodes.Zone.HumidRangeInvalid, "HumidMin cannot be greater than HumidMax", "humidMin"));

        return Result.Success();
    }

    public async Task<Result<Zone>> Add(Zone entity)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<Zone>.Failure(access.Error!);

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.Error!);

        return await _createService.Add(entity);
    }

    public async Task<Result<Zone>> Update(Zone entity)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<Zone>.Failure(access.Error!);

        var existing = await _uow.Zones.GetAsync(entity.Id);
        if (existing is null)
            return Result<Zone>.Failure(Errors.NotFound(ErrorCodes.Zone.NotFound, "Not found", "zoneId", entity.Id));

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.Error!);

        return await _updateService.Update(entity);
    }

    public async Task<Result<Zone>> Get(int id)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<Zone>.Failure(access.Error!);

        return await _readService.Get(id);
    }

    public async Task<Result<IEnumerable<Zone>>> GetAll()
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<IEnumerable<Zone>>.Failure(access.Error!);

        return await _readService.GetAll();
    }

    public async Task<Result> Delete(int id)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return access;

        var existing = await _uow.Zones.GetAsync(id);
        if (existing is null)
            return Result.Failure(Errors.NotFound(ErrorCodes.Zone.NotFound, "Not found", "zoneId", id));

        var batches = await _uow.Batches.GetAllAsync();
        var linkedBatchIds = batches
            .Where(batch => batch.ZoneId == id)
            .Select(batch => batch.Id)
            .ToArray();

        if (linkedBatchIds.Length > 0)
        {
            return Result.Failure(Errors.Conflict(
                ErrorCodes.Zone.HasBatches,
                "Zone cannot be deleted because it has linked batches",
                new Dictionary<string, object?>
                {
                    ["zoneId"] = id,
                    ["batchIds"] = linkedBatchIds
                }));
        }

        return await _deleteService.Delete(id);
    }

    public async Task<Result<(IEnumerable<Zone> items, int totalCount)>> Search(string query, int limit, int offset)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<(IEnumerable<Zone> items, int totalCount)>.Failure(access.Error!);

        if (limit <= 0)
            return Result<(IEnumerable<Zone>, int)>.Failure(PagingErrors.InvalidLimit(ErrorCodes.Zone.InvalidSearchPaging));

        if (offset < 0)
            return Result<(IEnumerable<Zone>, int)>.Failure(PagingErrors.InvalidOffset(ErrorCodes.Zone.InvalidSearchPaging));

        var (items, totalCount) = await _uow.Zones.SearchAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Zone>, int)>.Success((items, totalCount));
    }

}
