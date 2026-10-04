using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// The user photo is kept in the object storage (MinIO). The tests change the photo of the regular test user,
/// so the photo of the administrator stays as it is.
/// </summary>
[Order(10)]
internal class UserProfileTests : BaseTest
{
    private const string PhotoRequest = "api/user-profile/get-user-photo";

    /// <summary>
    /// UserService reports a failed photo load with the <c>userService.failedToLoadPhoto</c> text in English or Russian.
    /// </summary>
    private static readonly string[] FailedToLoadPhotoTexts =
        ["Failed to load user photo", "Не удалось загрузить фото пользователя"];

    /// <summary>
    /// Path of the photo as the browser sees it: in the test container the test project is mounted
    /// into the Chrome node, see <see cref="TestSetup.BaseDirectory"/>.
    /// </summary>
    private static string PhotoPath => $"{TestSetup.BaseDirectory.TrimEnd('/', '\\')}/TestData/avatar.png";

    private ProfilePage Profile => new(Driver);

    [OneTimeTearDown]
    public void SignInAsAdmin() => SignInAs(TestSetup.Admin);

    [Test, Order(1)]
    public void TopBarProfileButtonOpensProfile()
    {
        SignInAs(TestSetup.User);

        TopBar.OpenProfile();

        Menu.WaitForPath(ConfigPage.Path);
        Profile.WaitShown();
    }

    [Test, Order(2)]
    public void UploadedPhotoIsShownInProfileAndTopBar()
    {
        SignInAs(TestSetup.User);
        var profile = Profile.Open();

        profile.UploadPhoto(PhotoPath);
        Assert.That(() => TopBar.AvatarImageSource, Does.StartWith("data:image").After(5).Seconds.PollEvery(200).MilliSeconds,
            "The top bar does not show the uploaded photo");

        profile.Reload();
        Assert.Multiple(() =>
        {
            Assert.That(() => profile.AvatarImageSource, Does.StartWith("data:image").After(5).Seconds.PollEvery(200).MilliSeconds,
                "The profile does not show the photo after a reload");
            Assert.That(() => TopBar.AvatarImageSource, Does.StartWith("data:image").After(5).Seconds.PollEvery(200).MilliSeconds,
                "The top bar does not show the photo after a reload");
        });
    }

    [Test, Order(3)]
    public void DeletedPhotoIsReplacedWithInitials()
    {
        SignInAs(TestSetup.User);
        var profile = Profile.Open();
        profile.WaitForResponse(PhotoRequest);
        if (!profile.HasPhoto)
            profile.UploadPhoto(PhotoPath);

        profile.DeletePhoto();
        Assert.Multiple(() =>
        {
            Assert.That(() => TopBar.AvatarImageSource, Is.Null.After(5).Seconds.PollEvery(200).MilliSeconds,
                "The top bar still shows the deleted photo");
            Assert.That(TopBar.AvatarText, Is.Not.Empty, "The top bar shows neither a photo nor initials");
        });

        profile.Reload();
        profile.WaitForResponse(PhotoRequest);
        Assert.Multiple(() =>
        {
            Assert.That(profile.AvatarImageSource, Is.Null, "The profile shows a photo after it was deleted");
            Assert.That(TopBar.AvatarImageSource, Is.Null, "The top bar shows a photo after it was deleted");
        });
    }

    [Test, Order(4)]
    public void FailedPhotoLoadIsReportedInInterfaceLanguage()
    {
        SignInAs(TestSetup.User);

        using (BrowserFaults.FailRequests(Driver, PhotoRequest))
        {
            // The photo is requested while the application starts, long before the profile is opened;
            // the message must not depend on the profile translations being loaded.
            Driver.Navigate().Refresh();
            Assert.That(new Toasts(Driver).WaitForErrorSummary(), Is.AnyOf(FailedToLoadPhotoTexts));
        }

        Menu.Reload();
    }
}
