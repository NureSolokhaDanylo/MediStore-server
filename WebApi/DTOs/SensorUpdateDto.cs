using Domain.Enums;

namespace WebApi.DTOs;

public class SensorUpdateDto
{
    public int Id { get; set; }
    public string? SerialNumber { get; set; }
    public bool? IsOn { get; set; }
    public int? ZoneId { get; set; }
}
