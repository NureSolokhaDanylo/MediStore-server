namespace WebApi.DTOs;

public class ReadingCreateDto
{
    public required DateTime TimeStamp { get; set; }
    public required double Value { get; set; }
    public required int SensorId { get; set; }
}
