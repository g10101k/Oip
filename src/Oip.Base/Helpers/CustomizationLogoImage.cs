using System.Buffers.Binary;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Oip.Base.Controllers.Api;
using Oip.Base.Exceptions;

namespace Oip.Base.Helpers;

internal static partial class CustomizationLogoImage
{
    internal const string SvgContentSecurityPolicy =
        "sandbox; default-src 'none'; style-src 'unsafe-inline'; base-uri 'none'";

    private static ReadOnlySpan<byte> PngSignature => [137, 80, 78, 71, 13, 10, 26, 10];
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
    private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";

    private static readonly HashSet<string> SvgElements = new(StringComparer.Ordinal)
    {
        "svg", "g", "defs", "symbol", "use", "title", "desc", "path", "rect", "circle", "ellipse",
        "line", "polyline", "polygon", "text", "tspan", "textPath", "linearGradient", "radialGradient",
        "stop", "clipPath", "mask", "pattern", "marker", "filter", "feGaussianBlur", "feOffset",
        "feBlend", "feColorMatrix", "feComposite", "feFlood", "feMerge", "feMergeNode", "feDropShadow"
    };

    private static readonly HashSet<string> SvgAttributes = new(StringComparer.Ordinal)
    {
        "id", "class", "version", "viewBox", "preserveAspectRatio", "transform", "d", "points", "pathLength",
        "x", "y", "x1", "y1", "x2", "y2", "cx", "cy", "r", "rx", "ry", "fx", "fy", "fr",
        "width", "height", "dx", "dy", "rotate", "textLength", "lengthAdjust", "startOffset",
        "gradientUnits", "gradientTransform", "spreadMethod", "offset", "clipPathUnits", "maskUnits",
        "maskContentUnits", "patternUnits", "patternContentUnits", "patternTransform", "markerUnits",
        "markerWidth", "markerHeight", "orient", "refX", "refY", "filterUnits", "primitiveUnits",
        "in", "in2", "result", "stdDeviation", "edgeMode", "mode", "type", "values", "operator",
        "k1", "k2", "k3", "k4", "role", "aria-label", "aria-labelledby", "aria-hidden"
    };

    private static readonly HashSet<string> PresentationProperties = new(StringComparer.Ordinal)
    {
        "fill", "fill-opacity", "fill-rule", "stroke", "stroke-opacity", "stroke-width", "stroke-dasharray",
        "stroke-dashoffset", "stroke-linecap", "stroke-linejoin", "stroke-miterlimit", "color", "opacity",
        "clip-path", "clip-rule", "mask", "filter", "marker-start", "marker-mid", "marker-end",
        "stop-color", "stop-opacity", "flood-color", "flood-opacity", "display", "visibility", "overflow",
        "shape-rendering", "vector-effect", "paint-order", "font-family", "font-size", "font-weight",
        "font-style", "text-anchor", "dominant-baseline", "alignment-baseline", "letter-spacing"
    };

    internal static byte[]? Decode(string? base64)
    {
        if (base64 is null)
            return null;

        byte[] data;
        try
        {
            data = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw new ApiException("Invalid logo", "The logo must be Base64-encoded.",
                StatusCodes.Status400BadRequest);
        }

        if (data.Length > SiteCustomization.MaxLogoBytes || !(HasPngSignature(data) ? IsPng(data) : IsStaticSvg(data)))
            throw new ApiException("Invalid logo", "Upload a valid PNG or static SVG image up to 256 KiB.",
                StatusCodes.Status400BadRequest);

        return data;
    }

    internal static bool HasPngSignature(ReadOnlySpan<byte> data) => data.StartsWith(PngSignature);

    private static bool IsPng(ReadOnlySpan<byte> data)
    {
        var remaining = data[PngSignature.Length..];
        var hasHeader = false;
        var hasImageData = false;
        while (remaining.Length >= 12)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(remaining);
            if (length > remaining.Length - 12)
                return false;

            var chunk = remaining.Slice(4, (int)length + 4);
            remaining = remaining[(chunk.Length + 4)..];
            if (BinaryPrimitives.ReadUInt32BigEndian(remaining) != PngCrc(chunk))
                return false;
            remaining = remaining[4..];

            var type = chunk[..4];
            if (!hasHeader)
            {
                if (!type.SequenceEqual("IHDR"u8) || length != 13 ||
                    BinaryPrimitives.ReadUInt32BigEndian(chunk[4..]) is 0 or > int.MaxValue ||
                    BinaryPrimitives.ReadUInt32BigEndian(chunk[8..]) is 0 or > int.MaxValue)
                    return false;
                hasHeader = true;
            }
            else if (type.SequenceEqual("IHDR"u8))
                return false;
            else if (type.SequenceEqual("IDAT"u8) && length > 0)
                hasImageData = true;
            else if (type.SequenceEqual("IEND"u8))
                return length == 0 && hasImageData && remaining.IsEmpty;
        }

        return false;
    }

    private static uint PngCrc(ReadOnlySpan<byte> chunk)
    {
        var crc = uint.MaxValue;
        foreach (var value in chunk)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xedb88320u;
        }

        return ~crc;
    }

    internal static bool IsStaticSvg(byte[] data)
    {
        try
        {
            using var stream = new MemoryStream(data, writable: false);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = SiteCustomization.MaxLogoBytes
            });
            var document = XDocument.Load(reader);
            return document.Root?.Name == Svg + "svg" &&
                   !document.DescendantNodes().Any(node => node is XProcessingInstruction) &&
                   document.Descendants().All(IsSafeSvgElement);
        }
        catch (XmlException)
        {
            return false;
        }
    }

    private static bool IsSafeSvgElement(XElement element)
    {
        if (element.Name == Svg + "style")
            return element.Attributes().All(attribute => attribute.IsNamespaceDeclaration ||
                                                         (attribute.Name == "type" && attribute.Value == "text/css")) &&
                   element.Nodes().All(node => node is XText) &&
                   IsSafeStyleSheet(element.Value);

        return element.Name.Namespace == Svg && SvgElements.Contains(element.Name.LocalName) &&
               element.Attributes().All(IsSafeSvgAttribute);
    }

    private static bool IsSafeStyleSheet(string css)
    {
        var rules = css.Split('}', StringSplitOptions.TrimEntries);
        if (rules[^1].Length != 0)
            return false;

        foreach (var rule in rules.AsSpan(0, rules.Length - 1))
        {
            var brace = rule.IndexOf('{');
            if (brace <= 0 || !SimpleSelectors().IsMatch(rule[..brace]) || !IsSafeDeclarations(rule[(brace + 1)..]))
                return false;
        }

        return true;
    }

    private static bool IsSafeSvgAttribute(XAttribute attribute)
    {
        var name = attribute.Name;
        if (attribute.IsNamespaceDeclaration || name == XNamespace.Xml + "space")
            return true;
        if (name == "href" || name == XLink + "href")
            return LocalFragment().IsMatch(attribute.Value);
        if (name.Namespace != XNamespace.None)
            return false;
        if (name == "style")
            return IsSafeDeclarations(attribute.Value);
        return SvgAttributes.Contains(name.LocalName) || IsSafePresentationValue(name.LocalName, attribute.Value);
    }

    private static bool IsSafeDeclarations(string css) =>
        css.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(declaration =>
        {
            var colon = declaration.IndexOf(':');
            return colon > 0 && IsSafePresentationValue(declaration[..colon].Trim(), declaration[(colon + 1)..].Trim());
        });

    private static bool IsSafePresentationValue(string name, string value) =>
        PresentationProperties.Contains(name) &&
        (LocalPaintReference().IsMatch(value.Trim()) ||
         (name == "font-family" ? FontFamilyValue().IsMatch(value) : PlainPresentationValue().IsMatch(value)));

    [GeneratedRegex(@"\A#[a-zA-Z0-9_.:-]+\z")]
    private static partial Regex LocalFragment();

    [GeneratedRegex("""\Aurl\(\s*(?<quote>['"]?)#[a-zA-Z0-9_.:-]+\k<quote>\s*\)\z""", RegexOptions.IgnoreCase)]
    private static partial Regex LocalPaintReference();

    [GeneratedRegex(@"\A(?:[a-z0-9\s#.%+,/-]+|(?:rgba?|hsla?|hwb|lab|lch|oklab|oklch|color)\([a-z0-9\s.%+,/-]+\))\z",
        RegexOptions.IgnoreCase)]
    private static partial Regex PlainPresentationValue();

    [GeneratedRegex(@"\A[a-zA-Z0-9\s,'""-]+\z")]
    private static partial Regex FontFamilyValue();

    [GeneratedRegex(@"\A[a-zA-Z0-9\s_.#*,>+~-]+\z")]
    private static partial Regex SimpleSelectors();
}