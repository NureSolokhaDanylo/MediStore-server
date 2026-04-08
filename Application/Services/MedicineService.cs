using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class MedicineService : CrudService<Medicine>, IMedicineService
{
    private readonly IMedicineRepository _medicineRepository;

    public MedicineService(IMedicineRepository repository, IUnitOfWork uow) : base(repository, uow) 
    { 
        _medicineRepository = repository;
    }

    private Result Validate(Medicine m)
    {
        if (m.TempMin < -50 || m.TempMin > 100)
            return Result.Failure(new ErrorInfo
            {
                Code = "medicine.validation_failed",
                Message = "TempMin must be between -50 and 100",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "tempMin" }
            });

        if (m.TempMax < -50 || m.TempMax > 100)
            return Result.Failure(new ErrorInfo
            {
                Code = "medicine.validation_failed",
                Message = "TempMax must be between -50 and 100",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "tempMax" }
            });

        if (m.TempMin > m.TempMax)
            return Result.Failure(new ErrorInfo
            {
                Code = "medicine.validation_failed",
                Message = "TempMin cannot be greater than TempMax",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "tempMin" }
            });

        if (m.HumidMin < 0 || m.HumidMin > 100)
            return Result.Failure(new ErrorInfo
            {
                Code = "medicine.validation_failed",
                Message = "HumidMin must be between 0 and 100",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "humidMin" }
            });

        if (m.HumidMax < 0 || m.HumidMax > 100)
            return Result.Failure(new ErrorInfo
            {
                Code = "medicine.validation_failed",
                Message = "HumidMax must be between 0 and 100",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "humidMax" }
            });

        if (m.HumidMin > m.HumidMax)
            return Result.Failure(new ErrorInfo
            {
                Code = "medicine.validation_failed",
                Message = "HumidMin cannot be greater than HumidMax",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "humidMin" }
            });

        return Result.Success();
    }

    public override async Task<Result<Medicine>> Add(string userId, Medicine entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Medicine>.Failure(check.Error!);

        return await base.Add(userId, entity);
    }

    public override async Task<Result<Medicine>> Update(string userId, Medicine entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Medicine>.Failure(new ErrorInfo
            {
                Code = "medicine.not_found",
                Message = "Not found",
                Type = ErrorType.NotFound,
                Details = new Dictionary<string, object?> { ["medicineId"] = entity.Id }
            });

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Medicine>.Failure(check.Error!);

        return await base.Update(userId, entity);
    }

    public async Task<Result<(IEnumerable<Medicine> items, int totalCount)>> Search(string userId, string query, int limit, int offset)
    {
        if (limit <= 0)
            return Result<(IEnumerable<Medicine>, int)>.Failure(new ErrorInfo
            {
                Code = "medicine.invalid_search_paging",
                Message = "Limit must be greater than 0",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "limit" }
            });

        if (offset < 0)
            return Result<(IEnumerable<Medicine>, int)>.Failure(new ErrorInfo
            {
                Code = "medicine.invalid_search_paging",
                Message = "Offset cannot be negative",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "offset" }
            });

        var (items, totalCount) = await _medicineRepository.SearchAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Medicine>, int)>.Success((items, totalCount));
    }

    protected override async Task LogAsync(string userId, string action, Medicine? before, Medicine? after)
    {
        var beforeSnapshot = before is null
            ? null
            : new
            {
                before.Id,
                before.Name,
                before.Description,
                before.TempMax,
                before.TempMin,
                before.HumidMax,
                before.HumidMin,
                before.WarningThresholdDays
            };

        var afterSnapshot = after is null
            ? null
            : new
            {
                after.Id,
                after.Name,
                after.Description,
                after.TempMax,
                after.TempMin,
                after.HumidMax,
                after.HumidMin,
                after.WarningThresholdDays
            };

        var log = new AuditLog
        {
            OccurredAt = DateTime.UtcNow,
            EntityType = "Medicine",
            EntityId = after?.Id ?? before?.Id ?? 0,
            Action = action,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            Summary = $"Medicine {action} (Id={after?.Id ?? before?.Id})",
            OldValues = beforeSnapshot is null ? null : JsonSerializer.Serialize(beforeSnapshot),
            NewValues = afterSnapshot is null ? null : JsonSerializer.Serialize(afterSnapshot)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
