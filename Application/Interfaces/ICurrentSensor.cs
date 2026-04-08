namespace Application.Interfaces;

public interface ICurrentSensor
{
    bool IsAuthenticated { get; }
    int? SensorId { get; }
}
