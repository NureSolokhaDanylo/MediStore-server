namespace WebApi.DTOs;

public class BatchDto
{
    public required int Id { get; set; }
    public required string BatchNumber { get; set; }
    public required int Quantity { get; set; }
    public required DateTime ExpireDate { get; set; }
    public required DateTime DateAdded { get; set; }
    public required int MedicineId { get; set; }
    public required int ZoneId { get; set; }
}
