namespace WebApi.DTOs;

public class ZoneCreateDto
{
    public required string Name { get; set; } = null!;
    public string? Description { get; set; }
    public required double TempMax { get; set; }
    public required double TempMin { get; set; }
    public required double HumidMax { get; set; }
    public required double HumidMin { get; set; }
}
