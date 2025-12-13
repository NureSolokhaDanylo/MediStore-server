namespace WebApi.DTOs;

public class ReadingQueryDto
{
    public required int SensorId { get; set; }
    public required DateTime From { get; set; }
    public required DateTime To { get; set; }
}
