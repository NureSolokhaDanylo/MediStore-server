namespace WebApi.DTOs;

public class BatchSearchResultDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = null!;
    public int MedicineId { get; set; }
    public int ZoneId { get; set; }
}
