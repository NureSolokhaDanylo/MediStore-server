using Domain.Enums;

namespace WebApi.DTOs;

public class SensorCreateDto
{
    public required string SerialNumber { get; set; }
    public double? LastValue { get; set; }
    public DateTime? LastUpdate { get; set; }
    public required bool IsOn { get; set; }
    public required SensorType SensorType { get; set; }
    public int? ZoneId { get; set; }
}
