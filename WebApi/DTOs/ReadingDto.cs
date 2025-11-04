namespace WebApi.DTOs;

public class ReadingDto
{
 public int Id { get; set; }
 public DateTime TimeStamp { get; set; }
 public double Value { get; set; }
 public int SensorId { get; set; }
}
