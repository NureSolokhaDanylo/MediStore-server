using Domain.Enums;

namespace WebApi.DTOs;

public class AlertDto
{
    public required int Id { get; set; }
    public required string Message { get; set; }
    public required bool IsSolved { get; set; }
    public required DateTime CreationTime { get; set; }
    public DateTime? SolveTime { get; set; }
    public required AlertType AlertType { get; set; }
    public int? SensorId { get; set; }
    public int? BatchId { get; set; }
    public int? ZoneId { get; set; }
}
