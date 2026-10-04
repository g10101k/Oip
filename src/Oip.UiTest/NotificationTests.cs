using System.Text.Json;
using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// The notification bell. Notifications are created with the administrator endpoint that sends a test
/// notification to the current user; the fixture marks the ones it created as read when it is done.
/// </summary>
[Order(11)]
internal class NotificationTests : BaseTest
{
    private const string SubjectPrefix = "UiTestNotification";

    private NotificationBell Bell => new(Driver);
    private BackendApi Api => new(Driver);

    [OneTimeSetUp]
    public void SignInAsAdmin()
    {
        SignInAs(TestSetup.Admin);
        MarkTestNotificationsAsRead();
    }

    [OneTimeTearDown]
    public void MarkCreatedNotificationsAsRead()
    {
        SignInAs(TestSetup.Admin);
        MarkTestNotificationsAsRead();
    }

    [Test]
    public void NewNotificationArrivesWithoutReload()
    {
        SignInAs(TestSetup.Admin);
        var unread = Bell.UnreadCount;
        var subject = NewSubject();

        CreateNotification(subject);

        Assert.That(() => Bell.UnreadCount, Is.EqualTo(unread + 1).After(10).Seconds.PollEvery(200).MilliSeconds,
            "The unread count is not updated by the live delivery");
        // The live delivery also shows the notification as a toast, which stays until it is closed.
        Menu.Reload();
        Assert.That(Bell.Open().GetSubjects(), Has.Member(subject));
    }

    [Test]
    public void NotificationsArePaged()
    {
        SignInAs(TestSetup.Admin);
        // One more than the five notifications of a page; the newest come first.
        var subjects = Enumerable.Range(1, 6).Select(_ => NewSubject()).ToList();
        subjects.ForEach(CreateNotification);
        Menu.Reload();

        var bell = Bell.Open();
        Assert.That(bell.GetSubjects(), Is.EqualTo(subjects.Skip(1).Reverse()));

        bell.NextPage();
        Assert.That(bell.GetSubjects().First(), Is.EqualTo(subjects[0]));
    }

    [Test]
    public void ReadNotificationLeavesList()
    {
        SignInAs(TestSetup.Admin);
        var subject = NewSubject();
        CreateNotification(subject);
        Menu.Reload();
        var unread = Bell.UnreadCount;

        Bell.Open().MarkAsRead(subject);

        Assert.Multiple(() =>
        {
            Assert.That(Bell.GetSubjects(), Has.No.Member(subject));
            Assert.That(() => Bell.UnreadCount, Is.EqualTo(unread - 1).After(5).Seconds.PollEvery(200).MilliSeconds);
        });
    }

    private static string NewSubject() => $"{SubjectPrefix}-{Guid.NewGuid():N}"[..(SubjectPrefix.Length + 9)];

    private void CreateNotification(string subject) =>
        Api.Post("api/notification/create-test-notification",
            new { subject, message = "Sent by the notification UI tests." });

    private void MarkTestNotificationsAsRead()
    {
        var notifications = Api.Get("api/notification/get-notification-by-user?take=100&unreadOnly=true")
            .GetProperty("notifications").EnumerateArray()
            .Where(n => n.GetProperty("subject").GetString()?.StartsWith(SubjectPrefix) == true);
        foreach (var notification in notifications)
            Api.Post($"api/notification/mark-notification-as-read/{notification.GetProperty("notificationUserId").GetInt64()}");
    }
}
