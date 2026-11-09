using System.Text;
using Oip.Base.Exceptions;
using Oip.Base.Helpers;

namespace Oip.Test;

[TestFixture]
public class CustomizationLogoImageTests
{
    private const string Png =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGP4z8AAAAMBAQDJ/pLvAAAAAElFTkSuQmCC";

    [Test]
    public void AcceptsPngAndRejectsCorruptedChunk()
    {
        var bytes = Convert.FromBase64String(Png);
        Assert.That(CustomizationLogoImage.Decode(Png), Is.EqualTo(bytes));

        bytes[45] ^= 1;
        AssertInvalid(Convert.ToBase64String(bytes));
    }

    [Test]
    public void AcceptsSvgWithLocalReferences()
    {
        const string svg = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 100">
              <defs>
                <linearGradient id="paint"><stop stop-color="#fff"/></linearGradient>
                <rect id="mark" width="400" height="100"/>
              </defs>
              <style>.logo { fill: url('#paint'); }</style>
              <use class="logo" href="#mark" style="opacity:0.9"/>
            </svg>
            """;
        Assert.That(CustomizationLogoImage.Decode(Encode(svg)), Is.EqualTo(Encoding.UTF8.GetBytes(svg)));
    }

    [TestCase("<script/>")]
    [TestCase("<rect onload='alert(1)'/>")]
    [TestCase("<foreignObject/>")]
    [TestCase("<use href='https://example.com/logo.svg#mark'/>")]
    [TestCase("<use xml:base='https://example.com/logo.svg' href='#mark'/>")]
    [TestCase("<rect style='fill:URL(https://example.com/logo.svg#mark)'/>")]
    [TestCase("<rect style='fill:u&#92;72l(remote.svg)'/>")]
    [TestCase("<style>@import 'https://example.com/logo.css';</style>")]
    [TestCase("<style>.mark { fill: url(https://example.com/logo.svg#mark); }</style>")]
    [TestCase("<animate attributeName='href' to='https://example.com'/>")]
    public void RejectsActiveOrExternalSvgContent(string content) =>
        AssertInvalid(Encode($"""<svg xmlns="http://www.w3.org/2000/svg">{content}</svg>"""));

    [Test]
    public void RejectsInvalidDocumentsAndPayloads()
    {
        AssertInvalid("invalid base64");
        AssertInvalid(Encode("<svg"));
        AssertInvalid(Encode("""<!DOCTYPE svg><svg xmlns="http://www.w3.org/2000/svg"/>"""));
        AssertInvalid(Encode("""
            <?xml-stylesheet href="https://example.com/logo.css"?>
            <svg xmlns="http://www.w3.org/2000/svg"/>
            """));
        AssertInvalid(Encode($"""<svg xmlns="http://www.w3.org/2000/svg"><desc>{new string('a', 256 * 1024)}</desc></svg>"""));
    }

    private static string Encode(string svg) => Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

    private static void AssertInvalid(string base64) =>
        Assert.That(Assert.Throws<ApiException>(() => CustomizationLogoImage.Decode(base64))!.StatusCode,
            Is.EqualTo(400));
}
