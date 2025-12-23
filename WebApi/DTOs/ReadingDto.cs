namespace WebApi.DTOs;

public class ReadingDto
{
    public required int Id { get; set; }
    public required DateTime TimeStamp { get; set; }
    public required double Value { get; set; }
    //public required int? SensorId { get; set; }
    //public required int? ZoneId { get; set; }
}
