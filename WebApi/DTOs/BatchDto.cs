namespace WebApi.DTOs;

public class BatchDto
{
 public int Id { get; set; }
 public string BatchNumber { get; set; } = null!;
 public int Quantity { get; set; }
 public DateTime ExpireDate { get; set; }
 public DateTime DateAdded { get; set; }
 public int MedicineId { get; set; }
 public int ZoneId { get; set; }
}
