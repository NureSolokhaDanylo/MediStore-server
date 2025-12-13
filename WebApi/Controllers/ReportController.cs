using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    public class ReportController : MyController
    {
        private readonly IReportService _reportService;
        private readonly IReportDocumentService _docService;

        public ReportController(IReportService reportService, IReportDocumentService docService)
        {
            _reportService = reportService;
            _docService = docService;
        }

        // GET /api/v1/reports/alerts?from=2025-12-01T00:00:00Z&to=2025-12-02T00:00:00Z
        [HttpGet("alerts")]
        [Authorize(Roles = "Admin,Observer")]
        public async Task<IActionResult> AlertsReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var res = await _reportService.GetAlertsReportAsync(from, to);
            if (!res.IsSucceed) return BadRequest(res.ErrorMessage);

            var dto = res.Value!;

            var pdfBytes = _docService.GenerateAlertsPdf(dto, from, to);
            return File(pdfBytes, "application/pdf", $"alerts_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
