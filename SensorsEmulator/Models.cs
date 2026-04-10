public enum SensorType
{
    Temperature,
    Humidity
}

public class SensorConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public string? ApiBaseUrl { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public int? IntervalMs { get; set; }
}

public class ReadingDto
{
    public DateTime TimeStamp { get; set; }
    public double Value { get; set; }
}
