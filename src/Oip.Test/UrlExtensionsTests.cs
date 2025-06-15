using Oip.Base.Helpers;

namespace Oip.Test;

/// <summary>
/// Contains unit tests for <see cref="UrlExtensions"/> methods.
/// </summary>
[TestFixture]
public class UrlExtensionsTests
{
    /// <summary>
    /// Tests that the UrlAppend method correctly joins URL parts with various slash combinations.
    /// </summary>
    /// <param name="baseUrl">The base URL to test</param>
    /// <param name="part">The URL part to append</param>
    /// <returns>The properly combined URL string</returns>
    /// <remarks>
    /// This test covers multiple scenarios:
    /// - Base URL without trailing slash and part without leading slash
    /// - Base URL with trailing slash and part without leading slash
    /// - Base URL without trailing slash and part with leading slash
    /// - Base URL with trailing slash and part with leading slash
    /// - Empty part case (should add trailing slash)
    /// </remarks>
    [TestCase("https://example.com/api", "users", ExpectedResult = "https://example.com/api/users")]
    [TestCase("https://example.com/api/", "users", ExpectedResult = "https://example.com/api/users")]
    [TestCase("https://example.com/api", "/users", ExpectedResult = "https://example.com/api/users")]
    [TestCase("https://example.com/api/", "/users", ExpectedResult = "https://example.com/api/users")]
    [TestCase("https://example.com/api", "", ExpectedResult = "https://example.com/api/")]
    public string UrlAppend_ShouldCorrectlyJoinUrls(string baseUrl, string part)
    {
        // Act
        return baseUrl.UrlAppend(part);
    }
}