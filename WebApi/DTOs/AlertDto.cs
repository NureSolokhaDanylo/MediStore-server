using Domain.Enums;

namespace WebApi.DTOs;

public class AlertDto
{
    public required int Id { get; set; }
    public required string Message { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required AlertType AlertType { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? BatchId { get; set; }
    public int? ZoneId { get; set; }
}
