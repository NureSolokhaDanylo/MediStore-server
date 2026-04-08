using Application.Interfaces;
using Domain.Models;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class BatchAuditService(IUnitOfWork uow, ICurrentUser currentUser) : IEntityAuditService<Batch>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUser _currentUser = currentUser;

    public Task LogCreateAsync(Batch entity) => LogAsync("Create", null, entity);

    public Task LogUpdateAsync(Batch before, Batch after) => LogAsync("Update", before, after);

    public Task LogDeleteAsync(Batch entity) => LogAsync("Delete", entity, null);

    private async Task LogAsync(string action, Batch? before, Batch? after)
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
            UserId = string.IsNullOrWhiteSpace(_currentUser.UserId) ? null : _currentUser.UserId,
            Summary = $"Batch {action} (Id={id})",
            OldValues = beforeSnapshot is null ? null : JsonSerializer.Serialize(beforeSnapshot),
            NewValues = afterSnapshot is null ? null : JsonSerializer.Serialize(afterSnapshot)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
