using Oip.Base.Helpers;
using Xunit;

namespace Oip.Test;

/// <summary>
/// Test class for UrlHelper extensions.
/// </summary>
public class UrlHelperTest
{
    /// <summary>
    /// Append part to url with separate /
    /// </summary>
    /// <param name="url">The url to append to.</param>
    /// <param name="part">The part to append.</param>
    /// <param name="expected">The appended url.</param>
    [Theory]
    [InlineData("https://example.com", "test", "https://example.com/test")]
    [InlineData("https://example.com/", "test", "https://example.com/test")]
    [InlineData("https://example.com", "/test", "https://example.com/test")]
    [InlineData("https://example.com/", "/test", "https://example.com/test")]
    [InlineData("https://example.com", "", "https://example.com/")]
    public void UrlAppendTest(string url, string part, string expected)
    {
        Assert.Equal(url.UrlAppend(part), expected);
    }
}