using System.Text;
using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;
using Oip.Reports.Base.Pipeline;

namespace Oip.Reports.Base.Rendering;

public class HtmlReportRenderer : IReportDocumentRenderer
{
    public ReportDocument Render(ReportContext context, ReportLayout layout, string cacheKey)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\" />");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.AppendLine($"<title>{ReportGeneratorUtils.Encode(layout.Title)}</title>");
        html.AppendLine("<style>");
        html.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;background:#f4f6f8;margin:0;padding:24px;color:#17212b;}");
        html.AppendLine(".report-shell{max-width:1100px;margin:0 auto;background:#fff;border-radius:20px;box-shadow:0 18px 60px rgba(15,23,42,.12);overflow:hidden;}");
        html.AppendLine(".report-head{padding:24px 28px;background:linear-gradient(135deg,#0f766e,#155e75);color:#fff;}");
        html.AppendLine(".report-body{padding:24px 28px 32px;}");
        html.AppendLine(".report-meta{font-size:13px;opacity:.8;margin-top:8px;}");
        html.AppendLine(".report-table{width:100%;border-collapse:collapse;}");
        html.AppendLine(".report-table th,.report-table td{padding:12px 14px;border-bottom:1px solid #e2e8f0;text-align:left;vertical-align:top;}");
        html.AppendLine(".report-table th{font-size:12px;text-transform:uppercase;letter-spacing:.08em;color:#475569;background:#f8fafc;}");
        html.AppendLine(".report-footer{margin-top:18px;padding-top:16px;border-top:2px solid #e2e8f0;font-weight:600;color:#0f172a;}");
        html.AppendLine(".text-right{text-align:right;}.text-center{text-align:center;}.muted{color:#64748b;}.eyebrow{font-size:12px;letter-spacing:.08em;text-transform:uppercase;opacity:.75;}");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class=\"report-shell\">");
        html.AppendLine("<div class=\"report-head\">");
        html.AppendLine($"<div class=\"eyebrow\">Report Preview</div><h1>{ReportGeneratorUtils.Encode(layout.Title)}</h1>");
        html.AppendLine($"<div class=\"report-meta\">Generated in {ReportGeneratorUtils.Encode(context.Request.UserContext.GetValueOrDefault("timezone") ?? "UTC")} | Rows: {context.DataSet.Rows.Count}</div>");
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"report-body\">");

        var header = layout.Sections.FirstOrDefault(x => x.Type == ReportBandType.Header);
        if (header is not null)
        {
            foreach (var row in header.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    html.AppendLine(cell.IsHtml
                        ? cell.Text
                        : $"<p class=\"{cell.CssClass}\">{ReportGeneratorUtils.Encode(cell.Text)}</p>");
                }
            }
        }

        var detail = layout.Sections.FirstOrDefault(x => x.Type == ReportBandType.Detail);
        if (detail is not null && detail.Rows.Count > 0)
        {
            html.AppendLine("<table class=\"report-table\">");
            html.AppendLine("<thead><tr>");
            var detailBand = context.TemplateVersion.Definition.Bands.First(x => x.Type == ReportBandType.Detail);
            foreach (var element in detailBand.Elements)
            {
                html.AppendLine($"<th>{ReportGeneratorUtils.Encode(element.Label ?? element.ValuePath ?? string.Empty)}</th>");
            }

            html.AppendLine("</tr></thead>");
            html.AppendLine("<tbody>");
            foreach (var row in detail.Rows)
            {
                html.AppendLine("<tr>");
                foreach (var cell in row.Cells)
                {
                    var cssClass = string.Join(" ", new[]
                    {
                        cell.CssClass,
                        cell.Align switch
                        {
                            "right" => "text-right",
                            "center" => "text-center",
                            _ => null
                        }
                    }.Where(x => !string.IsNullOrWhiteSpace(x)));

                    html.AppendLine(cell.IsHtml
                        ? $"<td class=\"{cssClass}\">{cell.Text}</td>"
                        : $"<td class=\"{cssClass}\">{ReportGeneratorUtils.Encode(cell.Text)}</td>");
                }

                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
        }

        var footer = layout.Sections.FirstOrDefault(x => x.Type == ReportBandType.Footer);
        if (footer is not null)
        {
            html.AppendLine("<div class=\"report-footer\">");
            foreach (var row in footer.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    html.AppendLine(cell.IsHtml
                        ? cell.Text
                        : $"<div class=\"{cell.CssClass}\">{ReportGeneratorUtils.Encode(cell.Text)}</div>");
                }
            }

            html.AppendLine("</div>");
        }

        html.AppendLine("</div></div></body></html>");

        return new ReportDocument
        {
            CacheKey = cacheKey,
            FileName = $"{context.TemplateVersion.Definition.Id}-v{context.TemplateVersion.Version}.html",
            Html = html.ToString(),
            GeneratedAtUtc = DateTime.UtcNow
        };
    }
}
