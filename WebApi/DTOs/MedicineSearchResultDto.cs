namespace WebApi.DTOs;

public class MedicineSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
