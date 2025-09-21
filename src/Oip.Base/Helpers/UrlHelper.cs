namespace Oip.Base.Helpers;

/// <summary>
/// Url helper
/// </summary>
public static class UrlHelper
{
    /// <summary>
    /// Appends a part to the end of the URL, trimming both URL and part.
    /// </summary>
    /// <param name="url">The base URL.</param>
    /// <param name="part">The part to append.</param>
    /// <return>The combined URL.</return>
    public static string UrlAppend(this string url, string part)
    {
        ArgumentNullException.ThrowIfNull(part);
        return $"{url.TrimEnd('/')}/{part.TrimStart('/')}";
    }
}