using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class BatchService : CrudService<Batch>, IBatchService
{
    public BatchService(IBatchRepository repository, IUnitOfWork uow) : base(repository, uow) { }

    private Result Validate(Batch b)
    {
        if (b.Quantity <= 0)
            return Result.Failure("Quantity must be greater than 0");

        var now = DateTime.UtcNow;
        if (b.DateAdded.ToUniversalTime() > now.AddMinutes(1))
            return Result.Failure("DateAdded cannot be in the future");

        if (b.ExpireDate.ToUniversalTime() < b.DateAdded.ToUniversalTime())
            return Result.Failure("ExpireDate must be after DateAdded");

        return Result.Success();
    }

    public override async Task<Result<Batch>> Add(string userId, Batch entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.ErrorMessage ?? "Validation failed");

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure("Referenced medicine not found");

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure("Referenced zone not found");

        return await base.Add(userId, entity);
    }

    public override async Task<Result<Batch>> Update(string userId, Batch entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Batch>.Failure("Not found");

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.ErrorMessage ?? "Validation failed");

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure("Referenced medicine not found");

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure("Referenced zone not found");

        return await base.Update(userId, entity);
    }

    protected override async Task LogAsync(string userId, string action, Batch? before, Batch? after)
    {
        var id = after?.Id ?? before?.Id ?? 0;
        var log = new AuditLog
        {
            OccurredAt = DateTime.UtcNow,
            EntityType = "Batch",
            EntityId = id,
            Action = action,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            Summary = $"Batch {action} (Id={id})",
            OldValues = before is null ? null : JsonSerializer.Serialize(before),
            NewValues = after is null ? null : JsonSerializer.Serialize(after)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
