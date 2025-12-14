namespace WebApi.DTOs;

public class AppSettingsUpdateDto
{
    public required int Id { get; set; }
    public required bool AlertEnabled { get; set; }
    public required double TempAlertDeviation { get; set; }
    public required double HumidityAlertDeviation { get; set; }
    public required TimeSpan CheckDeviationInterval { get; set; }
}
