using Oip.Base.Helpers;

namespace Oip.Test;

/// <summary>
/// Test class for UrlHelper extensions.
/// </summary>
[TestFixture]
[TestOf(typeof(UrlHelper))]
public class UrlHelperTest
{
    /// <summary>
    /// Append part to url with separate /
    /// </summary>
    /// <param name="url">The url to append to.</param>
    /// <param name="part">The part to append.</param>
    /// <param name="expected">The appended url.</param>
    [TestCase("https://example.com", "test", "https://example.com/test")]
    [TestCase("https://example.com/", "test", "https://example.com/test")]
    [TestCase("https://example.com", "/test", "https://example.com/test")]
    [TestCase("https://example.com/", "/test", "https://example.com/test")]
    [TestCase("https://example.com", "test", "https://example.com/test")]
    [TestCase("https://example.com", "test", "https://example.com/test")]
    [TestCase("https://example.com", null, "https://example.com")]
    public void UrlAppendTest(string url, string? part, string expected)
    {
        Assert.That(url.UrlAppend(part), Is.EqualTo(expected));
    } 
}