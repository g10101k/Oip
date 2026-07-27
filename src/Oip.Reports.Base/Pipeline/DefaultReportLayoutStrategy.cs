using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;

namespace Oip.Reports.Base.Pipeline;

public class DefaultReportLayoutStrategy : IReportLayoutStrategy
{
    public ReportLayout BuildLayout(ReportContext context)
    {
        var layout = new ReportLayout
        {
            Title = context.TemplateVersion.Definition.Name
        };

        foreach (var band in context.TemplateVersion.Definition.Bands.OrderBy(x => x.Type))
        {
            var section = new ReportLayoutSection
            {
                Type = band.Type,
                CssClass = ResolveCssClass(context.TemplateVersion.Definition, band.StyleId)
            };

            if (band.Type == ReportBandType.Detail)
            {
                foreach (var dataRow in context.DataSet.Rows)
                {
                    section.Rows.Add(new ReportLayoutRow
                    {
                        Cells = band.Elements.Select(element => new ReportLayoutCell
                        {
                            Text = ReportGeneratorUtils.ResolveElementValue(element, context, dataRow),
                            CssClass = ResolveCssClass(context.TemplateVersion.Definition, element.StyleId),
                            Width = element.Layout.Width,
                            Align = element.Align,
                            IsHtml = element.AllowHtml
                        }).ToList()
                    });
                }
            }
            else
            {
                section.Rows.Add(new ReportLayoutRow
                {
                    Cells = band.Elements.Select(element => new ReportLayoutCell
                    {
                        Text = ReportGeneratorUtils.ResolveElementValue(element, context),
                        CssClass = ResolveCssClass(context.TemplateVersion.Definition, element.StyleId),
                        Width = element.Layout.Width,
                        Align = element.Align,
                        IsHtml = element.AllowHtml
                    }).ToList()
                });
            }

            layout.Sections.Add(section);
        }

        return layout;
    }

    private static string? ResolveCssClass(ReportDefinition definition, string? styleId)
    {
        if (string.IsNullOrWhiteSpace(styleId))
            return null;

        return definition.Styles.FirstOrDefault(x => x.Id == styleId)?.CssClass;
    }
}
