public enum SensorType
{
    Temperature,
    Humidity
}

public class SensorConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public SensorType Type { get; set; }
}

public class ReadingDto
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}
