using System.Collections.Generic;
using Domain.Enums;

namespace Application.DTOs;

public class AlertsReportDto
{
    public Dictionary<AlertType, int> Totals { get; set; } = new();
    public List<Domain.Models.Alert> ExpirationSoon { get; set; } = new();
    public List<Domain.Models.Alert> Expired { get; set; } = new();
    public List<Domain.Models.Alert> BatchWarnings { get; set; } = new();
    public List<Domain.Models.Alert> ZoneAlerts { get; set; } = new();
}
