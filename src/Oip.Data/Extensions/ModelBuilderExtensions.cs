using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace Oip.Base.Data.Extensions;

/// <summary>
/// ModelBuilder extensions
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Add xml doc annotation
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="designTime"></param>
    public static void ApplyXmlDocumentation(this ModelBuilder modelBuilder, bool designTime = false)
    {
        if (!designTime) return;
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var type = entityType.ClrType;
            var xmlPath = Path.ChangeExtension(type.Assembly.Location, ".xml");
            if (!File.Exists(xmlPath)) continue;
            var xmlDoc = XDocument.Load(xmlPath);

            var memberName = $"T:{type.FullName}";
            var memberDoc = xmlDoc.Descendants("member")
                .FirstOrDefault(m => m.Attribute("name")?.Value == memberName);

            if (memberDoc != null)
            {
                var summary = memberDoc.Element("summary")?.Value.Trim();
                entityType.SetComment(summary);
            }

            foreach (var property in entityType.GetProperties())
            {
                var propInfo = type.GetProperty(property.Name);
                if (propInfo == null) continue;

                var propMemberName = $"P:{type.FullName}.{property.Name}";
                var propMemberDoc = xmlDoc.Descendants("member")
                    .FirstOrDefault(m => m.Attribute("name")?.Value == propMemberName);

                if (propMemberDoc != null)
                {
                    var propSummary = propMemberDoc.Element("summary")?.Value.Trim();
                    property.SetComment(propSummary);
                }
            }
        }
    }
}