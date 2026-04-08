using Application.Interfaces;
using Application.Results.Base;
using Application.DTOs;
using Infrastructure.UOW;

namespace Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;
    private readonly IAccessChecker _accessChecker;

    public ReportService(IUnitOfWork uow, IAccessChecker accessChecker)
    {
        _uow = uow;
        _accessChecker = accessChecker;
    }

    public async Task<Result<AlertsReportDto>> GetAlertsReportAsync(DateTime from, DateTime to)
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(["Admin", "Observer"]);
        if (!access.IsSucceed)
            return Result<AlertsReportDto>.Failure(access.Error!);

        if (from >= to) return Result<AlertsReportDto>.Failure(Errors.Validation(ErrorCodes.Report.InvalidTimeRange, "Invalid time range"));

        var alerts = (await _uow.Alerts.GetAllAsync())
            .Where(a => a.CreatedAt >= from.ToUniversalTime() && a.CreatedAt <= to.ToUniversalTime())
            .OrderBy(a => a.CreatedAt)
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
