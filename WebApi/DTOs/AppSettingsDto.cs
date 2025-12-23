namespace WebApi.DTOs;

public class AppSettingsDto
{
    public required bool AlertEnabled { get; set; }
    public required double TempAlertDeviation { get; set; }
    public required double HumidityAlertDeviation { get; set; }
    public required TimeSpan CheckDeviationInterval { get; set; }
    public required int ReadingsRetentionDays { get; set; }
}
