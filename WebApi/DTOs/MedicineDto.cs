namespace WebApi.DTOs;

public class MedicineDto
{
 public int Id { get; set; }
 public string Name { get; set; } = null!;
 public string? Description { get; set; }
 public double TempMax { get; set; }
 public double TempMin { get; set; }
 public double HumidMax { get; set; }
 public double HumidMin { get; set; }
}
