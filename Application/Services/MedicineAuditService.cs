using Application.Interfaces;
using Domain.Models;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class MedicineAuditService(IUnitOfWork uow) : IEntityAuditService<Medicine>
{
    private readonly IUnitOfWork _uow = uow;

    public Task LogCreateAsync(string userId, Medicine entity) => LogAsync(userId, "Create", null, entity);

    public Task LogUpdateAsync(string userId, Medicine before, Medicine after) => LogAsync(userId, "Update", before, after);

    public Task LogDeleteAsync(string userId, Medicine entity) => LogAsync(userId, "Delete", entity, null);

    private async Task LogAsync(string userId, string action, Medicine? before, Medicine? after)
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
