using System.Text;
using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;

namespace Oip.Reports.Base.Rendering;

public class HtmlReportExporter : IReportExporter
{
    public ReportExportFormat Format => ReportExportFormat.Html;

    public ReportExportResult Export(ReportResult reportResult)
    {
        return new ReportExportResult
        {
            JobId = reportResult.JobId,
            ReportId = reportResult.ReportId,
            Version = reportResult.Version,
            Status = reportResult.Status,
            IsCached = reportResult.IsCached,
            FileName = reportResult.Document?.FileName ?? $"{reportResult.ReportId}.html",
            ContentType = reportResult.Document?.ContentType ?? "text/html; charset=utf-8",
            ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(reportResult.Document?.Html ?? string.Empty)),
            ExecutionLog = reportResult.ExecutionLog
        };
    }
}
