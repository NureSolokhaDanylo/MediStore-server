using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class MedicineService : CrudService<Medicine>, IMedicineService
{
    public MedicineService(IMedicineRepository repository, IUnitOfWork uow) : base(repository, uow) { }

    private Result Validate(Medicine m)
    {
        if (m.TempMin < -50 || m.TempMin > 100)
            return Result.Failure("TempMin must be between -50 and 100");

        if (m.TempMax < -50 || m.TempMax > 100)
            return Result.Failure("TempMax must be between -50 and 100");

        if (m.TempMin > m.TempMax)
            return Result.Failure("TempMin cannot be greater than TempMax");

        if (m.HumidMin < 0 || m.HumidMin > 100)
            return Result.Failure("HumidMin must be between 0 and 100");

        if (m.HumidMax < 0 || m.HumidMax > 100)
            return Result.Failure("HumidMax must be between 0 and 100");

        if (m.HumidMin > m.HumidMax)
            return Result.Failure("HumidMin cannot be greater than HumidMax");

        return Result.Success();
    }

    public override async Task<Result<Medicine>> Add(string userId, Medicine entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Medicine>.Failure(check.ErrorMessage ?? "Validation failed");

        return await base.Add(userId, entity);
    }

    public override async Task<Result<Medicine>> Update(string userId, Medicine entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Medicine>.Failure("Not found");

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Medicine>.Failure(check.ErrorMessage ?? "Validation failed");

        return await base.Update(userId, entity);
    }

    protected override async Task LogAsync(string userId, string action, Medicine? before, Medicine? after)
    {
        var log = new AuditLog
        {
            OccurredAt = DateTime.UtcNow,
            EntityType = "Medicine",
            EntityId = after?.Id ?? before?.Id ?? 0,
            Action = action,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            Summary = $"Medicine {action} (Id={after?.Id ?? before?.Id})",
            OldValues = before is null ? null : JsonSerializer.Serialize(before),
            NewValues = after is null ? null : JsonSerializer.Serialize(after)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
