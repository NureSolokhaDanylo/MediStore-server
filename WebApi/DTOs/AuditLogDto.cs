using System;

namespace WebApi.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Summary { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
