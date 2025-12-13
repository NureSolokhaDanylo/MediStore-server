using Application.Interfaces;
using Application.Results.Base;
using Application.DTOs;
using Infrastructure.UOW;

namespace Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<AlertsReportDto>> GetAlertsReportAsync(DateTime from, DateTime to)
    {
        if (from >= to) return Result<AlertsReportDto>.Failure("Invalid time range");

        var alerts = (await _uow.Alerts.GetAllAsync())
            .Where(a => a.CreationTime >= from.ToUniversalTime() && a.CreationTime <= to.ToUniversalTime())
            .OrderBy(a => a.CreationTime)
            .ToList();

        var dto = new AlertsReportDto();
        dto.Totals = alerts.GroupBy(a => a.AlertType).ToDictionary(g => g.Key, g => g.Count());
        dto.ExpirationSoon = alerts.Where(a => a.AlertType == Domain.Enums.AlertType.ExpirationSoon).ToList();
        dto.Expired = alerts.Where(a => a.AlertType == Domain.Enums.AlertType.Expired).ToList();
        dto.BatchWarnings = alerts.Where(a => a.AlertType == Domain.Enums.AlertType.BatchConditionWarning).ToList();
        dto.ZoneAlerts = alerts.Where(a => a.AlertType == Domain.Enums.AlertType.ZoneConditionAlert).ToList();

        return Result<AlertsReportDto>.Success(dto);
    }
}
