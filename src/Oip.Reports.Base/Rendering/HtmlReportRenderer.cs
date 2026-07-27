using System.Text;
using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;
using Oip.Reports.Base.Pipeline;

namespace Oip.Reports.Base.Rendering;

public class HtmlReportRenderer : IReportDocumentRenderer
{
    public ReportDocument Render(ReportContext context, ReportLayout layout, string cacheKey)
    {
        var definition = context.TemplateVersion.Definition;
        var unit = ToCssUnit(definition.Page.Unit);
        var printableWidth = definition.Page.Width - definition.Page.Margins.Left - definition.Page.Margins.Right;
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\" />");
        html.AppendLine($"<title>{ReportGeneratorUtils.Encode(layout.Title)}</title>");
        html.AppendLine("<style>");
        html.AppendLine($"@page{{size:{definition.Page.Width}{unit} {definition.Page.Height}{unit};margin:{definition.Page.Margins.Top}{unit} {definition.Page.Margins.Right}{unit} {definition.Page.Margins.Bottom}{unit} {definition.Page.Margins.Left}{unit};}}");
        html.AppendLine("*{box-sizing:border-box}body{margin:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;color:#17212b}.report-page{width:max-content;min-width:100%;margin:0 auto;padding:20px;background:white}.report-band{position:relative;width:100%;overflow:hidden}.report-band.page-break-before{break-before:page}.report-band.page-break-after{break-after:page}.report-element{position:absolute;overflow:hidden}.report-line{height:0!important;border-top:1px solid #17212b}.report-rectangle{border:1px solid #17212b}.report-image{width:100%;height:100%;object-fit:contain}.detail-row{position:relative;width:100%}@media print{body{background:#fff}.report-page{padding:0}.repeat-on-page{position:running(report-repeat)}}");
        html.AppendLine("</style></head><body><main class=\"report-page\">");

        var rows = ApplyGrouping(context, definition.Bands);
        foreach (var band in definition.Bands)
        {
            if (!band.Visible || !IsVisible(band.DisplayCondition, context, null))
                continue;

            if (band.Type == ReportBandType.Detail)
            {
                foreach (var row in rows)
                    html.Append(RenderBand(band, context, row, printableWidth, unit));
                continue;
            }

            if (band.Type is ReportBandType.GroupHeader or ReportBandType.GroupFooter)
            {
                var group = band.Grouping;
                if (group is null)
                    continue;

                foreach (var groupRows in rows.GroupBy(row => row.GetValueOrDefault(group.Expression)))
                    html.Append(RenderBand(band, context, groupRows.First(), printableWidth, unit));
                continue;
            }

            html.Append(RenderBand(band, context, null, printableWidth, unit));
        }

        html.AppendLine("</main></body></html>");
        return new ReportDocument
        {
            CacheKey = cacheKey,
            FileName = $"{definition.Id}-v{context.TemplateVersion.Version}.html",
            Html = html.ToString(),
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    private static IEnumerable<Dictionary<string, object?>> ApplyGrouping(ReportContext context, IEnumerable<ReportBand> bands)
    {
        var grouping = bands.FirstOrDefault(x => x.Grouping is not null)?.Grouping;
        if (grouping is null)
            return context.DataSet.Rows;

        return grouping.SortDirection == ReportSortDirection.Descending
            ? context.DataSet.Rows.OrderByDescending(x => Convert.ToString(x.GetValueOrDefault(grouping.Expression)))
            : context.DataSet.Rows.OrderBy(x => Convert.ToString(x.GetValueOrDefault(grouping.Expression)));
    }

    private static string RenderBand(ReportBand band, ReportContext context, IDictionary<string, object?>? row, decimal printableWidth, string unit)
    {
        var breakClass = band.PageBreak switch
        {
            ReportPageBreak.Before => " page-break-before",
            ReportPageBreak.After => " page-break-after",
            ReportPageBreak.BeforeAndAfter => " page-break-before page-break-after",
            _ => string.Empty
        };
        var repeatClass = band.RepeatOnEachPage ? " repeat-on-page" : string.Empty;
        var containerClass = band.Type == ReportBandType.Detail ? "detail-row" : "report-band";
        var html = new StringBuilder($"<section class=\"{containerClass}{breakClass}{repeatClass}\" style=\"height:{band.Height}{unit};max-width:{printableWidth}{unit}\">");
        foreach (var element in band.Elements.OrderBy(x => x.Layout.ZIndex))
        {
            if (!IsVisible(band.DisplayCondition, context, row))
                continue;
            html.Append(RenderElement(element, context, row, context.TemplateVersion.Definition, unit));
        }
        html.AppendLine("</section>");
        return html.ToString();
    }

    private static string RenderElement(ReportElement element, ReportContext context, IDictionary<string, object?>? row, ReportDefinition definition, string unit)
    {
        var layout = element.Layout;
        var styles = $"left:{layout.X}{unit};top:{layout.Y}{unit};width:{layout.Width}{unit};height:{layout.Height}{unit};z-index:{layout.ZIndex};{ResolveStyle(definition, element.StyleId)}";
        var css = element.Type switch
        {
            ReportElementType.Line => "report-element report-line",
            ReportElementType.Rectangle => "report-element report-rectangle",
            _ => "report-element"
        };
        var value = element.Type switch
        {
            ReportElementType.Image => $"<img class=\"report-image\" src=\"{ReportGeneratorUtils.Encode(element.SourceUrl)}\" alt=\"{ReportGeneratorUtils.Encode(element.Label)}\" />",
            ReportElementType.Line or ReportElementType.Rectangle => string.Empty,
            _ when element.AllowHtml => ReportGeneratorUtils.ResolveElementValue(element, context, row),
            _ => ReportGeneratorUtils.Encode(ReportGeneratorUtils.ResolveElementValue(element, context, row))
        };
        return $"<div class=\"{css}\" style=\"{styles}\">{value}</div>";
    }

    private static string ResolveStyle(ReportDefinition definition, string? styleId)
    {
        var style = definition.Styles.FirstOrDefault(x => string.Equals(x.Id, styleId, StringComparison.OrdinalIgnoreCase));
        if (style is null)
            return string.Empty;

        var allowedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "font-family", "font-size", "font-weight", "font-style", "color", "background", "background-color",
            "border", "border-color", "border-width", "border-style", "text-align", "padding", "opacity"
        };
        return string.Concat(style.Properties
            .Where(x => allowedProperties.Contains(x.Key))
            .Select(x => $"{x.Key}:{x.Value};"));
    }

    private static bool IsVisible(string? condition, ReportContext context, IDictionary<string, object?>? row)
    {
        if (string.IsNullOrWhiteSpace(condition) || string.Equals(condition, "always", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(condition, "never", StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = condition.Split("==", StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return true;
        var left = parts[0].StartsWith("parameter:", StringComparison.OrdinalIgnoreCase)
            ? context.Parameters.GetValueOrDefault(parts[0]["parameter:".Length..])
            : parts[0].StartsWith("row:", StringComparison.OrdinalIgnoreCase) && row is not null && row.TryGetValue(parts[0]["row:".Length..], out var rowValue)
                ? Convert.ToString(rowValue)
                : null;
        return string.Equals(left, parts[1], StringComparison.OrdinalIgnoreCase);
    }

    private static string ToCssUnit(ReportMeasurementUnit unit) => unit switch
    {
        ReportMeasurementUnit.Inch => "in",
        ReportMeasurementUnit.Point => "pt",
        ReportMeasurementUnit.Pixel => "px",
        _ => "mm"
    };
}
