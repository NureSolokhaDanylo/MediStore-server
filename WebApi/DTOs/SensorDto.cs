using Domain.Enums;

namespace WebApi.DTOs;

public class SensorDto
{
 public int Id { get; set; }
 public string SerialNumber { get; set; } = null!;
 public double? LastValue { get; set; }
 public DateTime? LastUpdate { get; set; }
 public bool IsOn { get; set; }
 public SensorType SensorType { get; set; }
 public int? ZoneId { get; set; }
}
