using Oip.UiTest.Pages;

namespace Oip.UiTest;

[Order(12)]
internal class ApplicationsTests : BaseTest
{
    private const string Code = "uitest-application";
    private const string DisplayName = "UI test application";
    private const string RenamedDisplayName = "UI test application renamed";

    private static readonly string[] RequiredFieldsTexts =
    [
        "Fill code, name, Base URL and Internal Base URL",
        "Заполните код, название, Base URL и Internal Base URL"
    ];

    private ApplicationsPage Applications => new(Driver);

    [OneTimeSetUp]
    [OneTimeTearDown]
    public void DeleteTestApplication()
    {
        SignInAs(TestSetup.Admin);
        var applications = Applications.Open();
        if (applications.Contains(Code))
            applications.Delete(Code);
    }

    [Test, Order(1)]
    public void CreateApplication()
    {
        SignInAs(TestSetup.Admin);
        var applications = Applications.Open();
        if (applications.Contains(Code))
            applications.Delete(Code);

        applications.Create(Code, DisplayName, "https://uitest.invalid");
        applications.Reload();

        Assert.That(applications.GetDisplayName(Code), Is.EqualTo(DisplayName));
    }

    [Test, Order(2)]
    public void RenameApplication()
    {
        SignInAs(TestSetup.Admin);
        var applications = EnsureTestApplication();

        applications.Rename(Code, RenamedDisplayName);
        applications.Reload();

        Assert.That(applications.GetDisplayName(Code), Is.EqualTo(RenamedDisplayName));
    }

    [Test, Order(3)]
    public void DisabledApplicationStaysInRegistry()
    {
        SignInAs(TestSetup.Admin);
        var applications = EnsureTestApplication();

        applications.SetEnabled(Code, false);
        applications.Reload();

        Assert.Multiple(() =>
        {
            Assert.That(applications.Contains(Code), Is.True);
            Assert.That(applications.IsEnabled(Code), Is.False);
        });
    }

    [Test, Order(4)]
    public void EnableDisabledApplication()
    {
        SignInAs(TestSetup.Admin);
        var applications = EnsureTestApplication();
        if (applications.IsEnabled(Code))
            applications.SetEnabled(Code, false);

        applications.SetEnabled(Code, true);
        applications.Reload();

        Assert.That(applications.IsEnabled(Code), Is.True);
    }

    [Test, Order(5)]
    public void InternalUrlIsRequired()
    {
        SignInAs(TestSetup.Admin);
        var applications = Applications.Open();

        var error = applications.CreateWithoutInternalUrl(Code + "-no-internal", DisplayName, "https://uitest.invalid");

        Assert.That(error, Is.AnyOf(RequiredFieldsTexts));
    }

    [Test, Order(6)]
    public void DeleteApplication()
    {
        SignInAs(TestSetup.Admin);
        var applications = EnsureTestApplication();

        applications.Delete(Code);
        applications.Reload();

        Assert.That(applications.Contains(Code), Is.False);
    }

    private ApplicationsPage EnsureTestApplication()
    {
        var applications = Applications.Open();
        if (!applications.Contains(Code))
            applications.Create(Code, DisplayName, "https://uitest.invalid");
        return applications;
    }
}
