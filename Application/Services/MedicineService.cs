using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.UOW;

namespace Application.Services;

public class MedicineService : IMedicineService
{
    private static readonly string[] ReadRoles = ["Admin", "Operator", "Observer"];
    private readonly IReadOnlyService<Medicine> _readService;
    private readonly ICreateService<Medicine> _createService;
    private readonly IUpdateService<Medicine> _updateService;
    private readonly IDeleteService<Medicine> _deleteService;
    private readonly IUnitOfWork _uow;
    private readonly IAccessChecker _accessChecker;

    public MedicineService(
        IReadOnlyService<Medicine> readService,
        ICreateService<Medicine> createService,
        IUpdateService<Medicine> updateService,
        IDeleteService<Medicine> deleteService,
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

    private Result Validate(Medicine m)
    {
        if (m.TempMin < -50 || m.TempMin > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.TempMinOutOfRange, "TempMin must be between -50 and 100", "tempMin"));

        if (m.TempMax < -50 || m.TempMax > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.TempMaxOutOfRange, "TempMax must be between -50 and 100", "tempMax"));

        if (m.TempMin > m.TempMax)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.TempRangeInvalid, "TempMin cannot be greater than TempMax", "tempMin"));

        if (m.HumidMin < 0 || m.HumidMin > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.HumidMinOutOfRange, "HumidMin must be between 0 and 100", "humidMin"));

        if (m.HumidMax < 0 || m.HumidMax > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.HumidMaxOutOfRange, "HumidMax must be between 0 and 100", "humidMax"));

        if (m.HumidMin > m.HumidMax)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.HumidRangeInvalid, "HumidMin cannot be greater than HumidMax", "humidMin"));

        return Result.Success();
    }

    public async Task<Result<Medicine>> Add(Medicine entity)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<Medicine>.Failure(access.Error!);

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Medicine>.Failure(check.Error!);

        return await _createService.Add(entity);
    }

    public async Task<Result<Medicine>> Update(Medicine entity)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<Medicine>.Failure(access.Error!);

        var existing = await _uow.Medicines.GetAsync(entity.Id);
        if (existing is null)
            return Result<Medicine>.Failure(Errors.NotFound(ErrorCodes.Medicine.NotFound, "Not found", "medicineId", entity.Id));

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Medicine>.Failure(check.Error!);

        return await _updateService.Update(entity);
    }

    public async Task<Result<Medicine>> Get(int id)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<Medicine>.Failure(access.Error!);

        return await _readService.Get(id);
    }

    public async Task<Result<IEnumerable<Medicine>>> GetAll()
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<IEnumerable<Medicine>>.Failure(access.Error!);

        return await _readService.GetAll();
    }

    public async Task<Result> Delete(int id)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return access;

        var entity = await _uow.Medicines.GetAsync(id);
        if (entity is null)
            return Result.Failure(Errors.NotFound(ErrorCodes.Medicine.NotFound, "Not found", "medicineId", id));

        var batches = await _uow.Batches.GetAllAsync();
        var linkedBatchIds = batches
            .Where(batch => batch.MedicineId == id)
            .Select(batch => batch.Id)
            .ToArray();

        if (linkedBatchIds.Length > 0)
        {
            return Result.Failure(Errors.Conflict(
                ErrorCodes.Medicine.HasBatches,
                "Medicine cannot be deleted because it has linked batches",
                new Dictionary<string, object?>
                {
                    ["medicineId"] = id,
                    ["batchIds"] = linkedBatchIds
                }));
        }

        return await _deleteService.Delete(id);
    }

    public async Task<Result<(IEnumerable<Medicine> items, int totalCount)>> Search(string query, int limit, int offset)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(ReadRoles);
        if (!access.IsSucceed)
            return Result<(IEnumerable<Medicine> items, int totalCount)>.Failure(access.Error!);

        if (limit <= 0)
            return Result<(IEnumerable<Medicine>, int)>.Failure(PagingErrors.InvalidLimit(ErrorCodes.Medicine.InvalidSearchPaging));

        if (offset < 0)
            return Result<(IEnumerable<Medicine>, int)>.Failure(PagingErrors.InvalidOffset(ErrorCodes.Medicine.InvalidSearchPaging));

        var (items, totalCount) = await _uow.Medicines.SearchAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Medicine>, int)>.Success((items, totalCount));
    }

}
