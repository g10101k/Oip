using System.Text.RegularExpressions;

namespace Oip.UiTest.Pages;

/// <summary>
/// The notification bell of the top bar and its popover with the unread notifications of the current user.
/// </summary>
internal partial class NotificationBell(IWebDriver driver) : BasePage(driver)
{
    private static readonly By Button = By.Id("oip-app-topbar-notification-button");
    private static readonly By Popover = By.CssSelector(".p-popover");
    private static readonly By Subjects = By.CssSelector("[qa-id='oip-user-notifications-subject']");
    private static readonly By NextPageButton =
        By.CssSelector("[qa-id='oip-user-notifications-paginator'] .p-paginator-next");

    /// <summary>
    /// Unread notification count. The button carries it in its label, the badge is hidden at zero.
    /// </summary>
    public int UnreadCount
    {
        get
        {
            var label = Wait.UntilFindElement(Button).GetAttribute("aria-label") ?? string.Empty;
            return int.Parse(CountPattern().Match(label).Value);
        }
    }

    /// <summary>
    /// Opens the popover and waits until the first page of notifications has loaded.
    /// </summary>
    public NotificationBell Open()
    {
        Wait.UntilClick(Button);
        Wait.UntilFindElement(Popover);
        Wait.UntilFindElement(Subjects);
        return this;
    }

    /// <summary>
    /// Subjects of the notifications on the current page, newest first.
    /// </summary>
    public IReadOnlyList<string> GetSubjects() =>
        FindAllNow(Driver, Subjects).Select(subject => subject.Text.Trim()).ToList();

    /// <summary>
    /// Goes to the next page of notifications and waits until it has loaded.
    /// </summary>
    public NotificationBell NextPage()
    {
        var firstSubject = GetSubjects().FirstOrDefault();
        Wait.UntilClick(NextPageButton);
        Wait.Until(_ => GetSubjects().FirstOrDefault() is { } first && first != firstSubject);
        return this;
    }

    /// <summary>
    /// Marks the notification with the given subject as read and waits until it leaves the list.
    /// </summary>
    /// <param name="subject">Subject of the notification.</param>
    public void MarkAsRead(string subject)
    {
        var markAsRead = By.XPath(
            $"//*[@qa-id='oip-user-notifications-item'][.//*[@qa-id='oip-user-notifications-subject'][normalize-space()='{subject}']]" +
            "//*[@qa-id='oip-user-notifications-mark-as-read']");
        new Toasts(Driver).ExpectSuccess(() => Wait.UntilClick(markAsRead));
        Wait.UntilDisappear(markAsRead);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex CountPattern();
}
