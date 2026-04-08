using Application.Interfaces;
using Domain.Models;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class ZoneAuditService(IUnitOfWork uow) : IEntityAuditService<Zone>
{
    private readonly IUnitOfWork _uow = uow;

    public Task LogCreateAsync(string userId, Zone entity) => LogAsync(userId, "Create", null, entity);

    public Task LogUpdateAsync(string userId, Zone before, Zone after) => LogAsync(userId, "Update", before, after);

    public Task LogDeleteAsync(string userId, Zone entity) => LogAsync(userId, "Delete", entity, null);

    private async Task LogAsync(string userId, string action, Zone? before, Zone? after)
    {
        var id = after?.Id ?? before?.Id ?? 0;
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
                before.HumidMin
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
                after.HumidMin
            };

        var log = new AuditLog
        {
            OccurredAt = DateTime.UtcNow,
            EntityType = "Zone",
            EntityId = id,
            Action = action,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            Summary = $"Zone {action} (Id={id})",
            OldValues = beforeSnapshot is null ? null : JsonSerializer.Serialize(beforeSnapshot),
            NewValues = afterSnapshot is null ? null : JsonSerializer.Serialize(afterSnapshot)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
