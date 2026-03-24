namespace WebApi.DTOs;

public class AlertFilterDto
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
    public bool? IsActive { get; set; } = null; // null = all, true = active only, false = resolved only
    public int? ZoneId { get; set; } = null;
    public int? BatchId { get; set; } = null;
}