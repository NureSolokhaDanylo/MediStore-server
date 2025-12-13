using Application.DTOs;

namespace WebApi.Services
{
    public interface IReportDocumentService
    {
        byte[] GenerateAlertsPdf(AlertsReportDto dto, DateTime from, DateTime to);
    }
}
