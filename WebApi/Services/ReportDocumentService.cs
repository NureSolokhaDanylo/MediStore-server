using Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace WebApi.Services
{
    public class ReportDocumentService : IReportDocumentService
    {
        public byte[] GenerateAlertsPdf(AlertsReportDto dto, DateTime from, DateTime to)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text(text =>
                    {
                        text.Span("Alerts report").Bold().FontSize(18);
                        text.Span("\n");
                        text.Span($"From: {from.ToUniversalTime():yyyy-MM-dd HH:mm:ss} UTC To: {to.ToUniversalTime():yyyy-MM-dd HH:mm:ss} UTC").FontSize(10);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text("Summary:").Bold();
                        col.Item().Text(WriteSummary(dto.Totals));

                        col.Item().Text("\nExpirationSoon alerts:").Bold();
                        foreach (var a in dto.ExpirationSoon) col.Item().Text(FormatAlert(a));

                        col.Item().Text("\nExpired alerts:").Bold();
                        foreach (var a in dto.Expired) col.Item().Text(FormatAlert(a));

                        col.Item().Text("\nBatchConditionWarning alerts:").Bold();
                        foreach (var a in dto.BatchWarnings) col.Item().Text(FormatAlert(a));

                        col.Item().Text("\nZoneConditionAlert alerts:").Bold();
                        foreach (var a in dto.ZoneAlerts) col.Item().Text(FormatAlert(a));
                    });

                    page.Footer().AlignCenter().Text(x => x.Span($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC").FontSize(9));
                });
            });

            return doc.GeneratePdf();
        }

        private static string WriteSummary(Dictionary<Domain.Enums.AlertType, int> totals)
        {
            var sb = new StringBuilder();
            foreach (Domain.Enums.AlertType t in Enum.GetValues(typeof(Domain.Enums.AlertType)))
            {
                totals.TryGetValue(t, out var c);
                sb.AppendLine($"{t}: {c}");
            }
            return sb.ToString();
        }

        private static string FormatAlert(Domain.Models.Alert a)
        {
            var ids = new List<string>();
            if (a.BatchId.HasValue) ids.Add($"Batch:{a.BatchId}");
            if (a.ZoneId.HasValue) ids.Add($"Zone:{a.ZoneId}");
            if (a.SensorId.HasValue) ids.Add($"Sensor:{a.SensorId}");

            return $"[{a.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC] Type: {a.AlertType}; {string.Join(',', ids)}; Msg: {a.Message}";
        }
    }
}
