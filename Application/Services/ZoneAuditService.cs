using Application.Interfaces;
using Domain.Models;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class ZoneAuditService(IUnitOfWork uow, ICurrentUser currentUser) : IEntityAuditService<Zone>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUser _currentUser = currentUser;

    public Task LogCreateAsync(Zone entity) => LogAsync("Create", null, entity);

    public Task LogUpdateAsync(Zone before, Zone after) => LogAsync("Update", before, after);

    public Task LogDeleteAsync(Zone entity) => LogAsync("Delete", entity, null);

    private async Task LogAsync(string action, Zone? before, Zone? after)
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
            UserId = string.IsNullOrWhiteSpace(_currentUser.UserId) ? null : _currentUser.UserId,
            Summary = $"Zone {action} (Id={id})",
            OldValues = beforeSnapshot is null ? null : JsonSerializer.Serialize(beforeSnapshot),
            NewValues = afterSnapshot is null ? null : JsonSerializer.Serialize(afterSnapshot)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
