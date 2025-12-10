namespace WebApi.DTOs;

public class MedicineCreateDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required double TempMax { get; set; }
    public required double TempMin { get; set; }
    public required double HumidMax { get; set; }
    public required double HumidMin { get; set; }
}
