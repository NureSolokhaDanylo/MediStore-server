using Application.Results.Base;
using Application.DTOs;

namespace Application.Interfaces;

public interface IReportService
{
    Task<Result<AlertsReportDto>> GetAlertsReportAsync(DateTime from, DateTime to);
}
