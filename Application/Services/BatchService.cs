using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class BatchService : IBatchService
{
    private static readonly string[] ReadRoles = ["Admin", "Operator", "Observer"];
    private readonly IReadOnlyService<Batch> _readService;
    private readonly ICreateService<Batch> _createService;
    private readonly IUpdateService<Batch> _updateService;
    private readonly IDeleteService<Batch> _deleteService;
    private readonly IUnitOfWork _uow;
    private readonly IAccessChecker _accessChecker;

    public BatchService(
        IReadOnlyService<Batch> readService,
        ICreateService<Batch> createService,
        IUpdateService<Batch> updateService,
        IDeleteService<Batch> deleteService,
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

    private Result Validate(Batch b)
    {
        if (b.Quantity <= 0)
            return Result.Failure(Errors.Validation(ErrorCodes.Batch.QuantityMustBePositive, "Quantity must be greater than 0", "quantity"));

        var now = DateTime.UtcNow;
        if (b.DateAdded.ToUniversalTime() > now.AddMinutes(1))
            return Result.Failure(Errors.Validation(ErrorCodes.Batch.DateAddedInFuture, "DateAdded cannot be in the future", "dateAdded"));

        if (b.ExpireDate.ToUniversalTime() < b.DateAdded.ToUniversalTime())
            return Result.Failure(Errors.Validation(ErrorCodes.Batch.ExpireDateBeforeDateAdded, "ExpireDate must be after DateAdded", "expireDate"));

        return Result.Success();
    }

    public async Task<Result<Batch>> Add(Batch entity)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Operator");
        if (!access.IsSucceed)
            return Result<Batch>.Failure(access.Error!);

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.Error!);

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.MedicineNotFound, "Referenced medicine not found", "medicineId", entity.MedicineId));

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.ZoneNotFound, "Referenced zone not found", "zoneId", entity.ZoneId));

        return await _createService.Add(entity);
    }

    public async Task<Result<Batch>> Update(Batch entity)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Operator");
        if (!access.IsSucceed)
            return Result<Batch>.Failure(access.Error!);

        var existing = await _uow.Batches.GetAsync(entity.Id);
        if (existing is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.NotFound, "Not found", "batchId", entity.Id));

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.Error!);

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.MedicineNotFound, "Referenced medicine not found", "medicineId", entity.MedicineId));

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.ZoneNotFound, "Referenced zone not found", "zoneId", entity.ZoneId));

        return await _updateService.Update(entity);
    }

    public async Task<Result<Batch>> Get(int id)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<Batch>.Failure(access.Error!);

        return await _readService.Get(id);
    }

    public async Task<Result<IEnumerable<Batch>>> GetAll()
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<IEnumerable<Batch>>.Failure(access.Error!);

        return await _readService.GetAll();
    }

    public async Task<Result> Delete(int id)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Operator");
        if (!access.IsSucceed)
            return access;

        return await _deleteService.Delete(id);
    }

    public async Task<Result<(IEnumerable<Batch> items, int totalCount)>> SearchByBatchNumber(string query, int limit, int offset)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<(IEnumerable<Batch> items, int totalCount)>.Failure(access.Error!);

        if (limit <= 0)
            return Result<(IEnumerable<Batch>, int)>.Failure(PagingErrors.InvalidLimit(ErrorCodes.Batch.InvalidSearchPaging));

        if (offset < 0)
            return Result<(IEnumerable<Batch>, int)>.Failure(PagingErrors.InvalidOffset(ErrorCodes.Batch.InvalidSearchPaging));

        var (items, totalCount) = await _uow.Batches.SearchByBatchNumberAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Batch>, int)>.Success((items, totalCount));
    }

}
