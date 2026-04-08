using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class BatchService : CrudService<Batch>, IBatchService
{
    private readonly IBatchRepository _batchRepository;

    public BatchService(IBatchRepository repository, IUnitOfWork uow) : base(repository, uow) 
    { 
        _batchRepository = repository;
    }

    private Result Validate(Batch b)
    {
        if (b.Quantity <= 0)
            return Result.Failure(new ErrorInfo
            {
                Code = "batch.validation_failed",
                Message = "Quantity must be greater than 0",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "quantity" }
            });

        var now = DateTime.UtcNow;
        if (b.DateAdded.ToUniversalTime() > now.AddMinutes(1))
            return Result.Failure(new ErrorInfo
            {
                Code = "batch.validation_failed",
                Message = "DateAdded cannot be in the future",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "dateAdded" }
            });

        if (b.ExpireDate.ToUniversalTime() < b.DateAdded.ToUniversalTime())
            return Result.Failure(new ErrorInfo
            {
                Code = "batch.validation_failed",
                Message = "ExpireDate must be after DateAdded",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "expireDate" }
            });

        return Result.Success();
    }

    public override async Task<Result<Batch>> Add(string userId, Batch entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.Error!);

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure(new ErrorInfo
            {
                Code = "batch.medicine_not_found",
                Message = "Referenced medicine not found",
                Type = ErrorType.NotFound,
                Details = new Dictionary<string, object?> { ["medicineId"] = entity.MedicineId }
            });

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure(new ErrorInfo
            {
                Code = "batch.zone_not_found",
                Message = "Referenced zone not found",
                Type = ErrorType.NotFound,
                Details = new Dictionary<string, object?> { ["zoneId"] = entity.ZoneId }
            });

        return await base.Add(userId, entity);
    }

    public override async Task<Result<Batch>> Update(string userId, Batch entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Batch>.Failure(new ErrorInfo
            {
                Code = "batch.not_found",
                Message = "Not found",
                Type = ErrorType.NotFound,
                Details = new Dictionary<string, object?> { ["batchId"] = entity.Id }
            });

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.Error!);

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure(new ErrorInfo
            {
                Code = "batch.medicine_not_found",
                Message = "Referenced medicine not found",
                Type = ErrorType.NotFound,
                Details = new Dictionary<string, object?> { ["medicineId"] = entity.MedicineId }
            });

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure(new ErrorInfo
            {
                Code = "batch.zone_not_found",
                Message = "Referenced zone not found",
                Type = ErrorType.NotFound,
                Details = new Dictionary<string, object?> { ["zoneId"] = entity.ZoneId }
            });

        return await base.Update(userId, entity);
    }

    public async Task<Result<(IEnumerable<Batch> items, int totalCount)>> SearchByBatchNumber(string userId, string query, int limit, int offset)
    {
        if (limit <= 0)
            return Result<(IEnumerable<Batch>, int)>.Failure(new ErrorInfo
            {
                Code = "batch.invalid_search_paging",
                Message = "Limit must be greater than 0",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "limit" }
            });

        if (offset < 0)
            return Result<(IEnumerable<Batch>, int)>.Failure(new ErrorInfo
            {
                Code = "batch.invalid_search_paging",
                Message = "Offset cannot be negative",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "offset" }
            });

        var (items, totalCount) = await _batchRepository.SearchByBatchNumberAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Batch>, int)>.Success((items, totalCount));
    }

    protected override async Task LogAsync(string userId, string action, Batch? before, Batch? after)
    {
        var id = after?.Id ?? before?.Id ?? 0;
        var beforeSnapshot = before is null
            ? null
            : new
            {
                before.Id,
                before.BatchNumber,
                before.Quantity,
                before.ExpireDate,
                before.DateAdded,
                before.MedicineId,
                before.ZoneId
            };

        var afterSnapshot = after is null
            ? null
            : new
            {
                after.Id,
                after.BatchNumber,
                after.Quantity,
                after.ExpireDate,
                after.DateAdded,
                after.MedicineId,
                after.ZoneId
            };

        var log = new AuditLog
        {
            OccurredAt = DateTime.UtcNow,
            EntityType = "Batch",
            EntityId = id,
            Action = action,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            Summary = $"Batch {action} (Id={id})",
            OldValues = beforeSnapshot is null ? null : JsonSerializer.Serialize(beforeSnapshot),
            NewValues = afterSnapshot is null ? null : JsonSerializer.Serialize(afterSnapshot)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
