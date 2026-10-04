namespace Oip.UiTest.Pages;

/// <summary>
/// The profile block of the configuration page, which the top bar profile button opens: the photo of the
/// current user, kept in the object storage.
/// </summary>
internal class ProfilePage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By Avatar = By.Id("oip-user-profile-photo-avatar");
    private static readonly By AvatarImage = By.CssSelector("#oip-user-profile-photo-avatar img");
    private static readonly By FileInput = By.CssSelector("#oip-user-profile-file-upload input[type='file']");
    private static readonly By DeleteButton = By.Id("oip-user-profile-delete-photo");

    /// <summary>
    /// Opens the configuration page with the profile block.
    /// </summary>
    public ProfilePage Open()
    {
        Navigate(ConfigPage.Path);
        WaitForAppInteractive();
        return WaitShown();
    }

    /// <summary>
    /// Waits until the profile block is shown on the current page.
    /// </summary>
    public ProfilePage WaitShown()
    {
        Wait.UntilFindElement(Avatar);
        return this;
    }

    /// <summary>
    /// Whether the delete button is shown, i.e. the user has a photo.
    /// </summary>
    public bool HasPhoto => ExistsNow(DeleteButton);

    /// <summary>
    /// Source of the photo in the avatar, or null while the avatar is empty.
    /// </summary>
    public string? AvatarImageSource => ExistsNow(AvatarImage) ? Driver.FindElement(AvatarImage).GetAttribute("src") : null;

    /// <summary>
    /// Uploads the photo and waits until it is shown.
    /// </summary>
    /// <param name="path">Path of the image as the browser sees it, see <see cref="TestSetup.BaseDirectory"/>.</param>
    public void UploadPhoto(string path)
    {
        // The upload starts as soon as a file is chosen; the input is hidden but still accepts the path.
        new Toasts(Driver).ExpectSuccess(() => Driver.FindElement(FileInput).SendKeys(path));
        Wait.Until(_ => AvatarImageSource?.StartsWith("data:image") == true);
    }

    /// <summary>
    /// Deletes the photo, confirming it, and waits until the avatar is empty.
    /// </summary>
    public void DeletePhoto()
    {
        new Toasts(Driver).ExpectSuccess(() =>
        {
            Wait.UntilClick(DeleteButton);
            new ConfirmDialog(Driver).Accept();
        });
        Wait.UntilDisappear(AvatarImage);
    }
}
