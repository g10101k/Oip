using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// The discussion page. It shares its object with whatever else is discussed there, so the tests mark their
/// comments with a unique text and delete them. Attachments are kept in the object storage (MinIO).
/// </summary>
[Order(16)]
internal class DiscussionTests : BaseTest
{
    private const string Prefix = "UiTestComment";

    private static string AttachmentPath => $"{TestSetup.BaseDirectory.TrimEnd('/', '\\')}/TestData/avatar.png";

    private readonly List<string> _posted = [];

    private DiscussionPage Discussion => new(Driver);

    [OneTimeTearDown]
    public void DeletePostedComments()
    {
        SignInAs(TestSetup.Admin);
        var discussion = Discussion.Open();
        foreach (var text in _posted.Where(discussion.Contains))
            discussion.Delete(text);
    }

    [Test]
    public void EditedCommentKeepsHistory()
    {
        var discussion = OpenDiscussion();
        var text = NewText();
        var editedText = NewText();
        Post(discussion, text);

        discussion.Edit(text, editedText);
        _posted.Add(editedText);

        Assert.That(discussion.IsEdited(editedText), Is.True, "The comment is not marked as edited");
        Assert.That(discussion.GetFirstEdit(editedText), Is.EqualTo((text, editedText)));
    }

    [Test]
    public void DeletedCommentDisappears()
    {
        var discussion = OpenDiscussion();
        var text = NewText();
        Post(discussion, text);

        discussion.Delete(text);
        discussion.Reload();

        Assert.That(discussion.Contains(text), Is.False);
    }

    [Test]
    public void MentionIsHighlighted()
    {
        var discussion = OpenDiscussion();
        var text = NewText();

        var posted = discussion.PostWithMention(text, TestSetup.User.Username);
        _posted.Add(text);

        // The suggestion inserts the e-mail of the user, which the comment renders as a mention.
        var mention = posted[(posted.IndexOf('@'))..];
        Assert.That(discussion.GetMentions(text), Is.EqualTo(new[] { mention }));
    }

    [Test]
    public void ReactionCanBeAddedAndTakenBack()
    {
        var discussion = OpenDiscussion();
        var text = NewText();
        Post(discussion, text);

        var reaction = discussion.React(text);
        Assert.That(reaction, Does.EndWith(" 1"), "The reaction of the current user is expected to be counted once");

        discussion.Reload();
        Assert.That(discussion.GetReactions(text), Is.EqualTo(new[] { reaction }), "The reaction is not kept after a reload");

        discussion.Unreact(text);
        Assert.That(discussion.GetReactions(text), Is.Empty);
    }

    [Test]
    public void AttachmentIsUploadedAndDeleted()
    {
        var discussion = OpenDiscussion();
        var text = NewText();
        Post(discussion, text, AttachmentPath);

        Assert.That(() => discussion.GetAttachments(text), Is.EqualTo(new[] { "avatar.png" }).After(5).Seconds.PollEvery(200).MilliSeconds);
        discussion.Reload();
        Assert.That(discussion.GetAttachments(text), Is.EqualTo(new[] { "avatar.png" }), "The attachment is not kept after a reload");

        discussion.DeleteAttachment(text);
        discussion.Reload();
        Assert.That(discussion.GetAttachments(text), Is.Empty);
    }

    private DiscussionPage OpenDiscussion()
    {
        SignInAs(TestSetup.Admin);
        return Discussion.Open();
    }

    private void Post(DiscussionPage discussion, string text, string? attachmentPath = null)
    {
        discussion.Post(text, attachmentPath);
        _posted.Add(text);
    }

    // Comments are rendered as Markdown: no characters with a meaning there.
    private static string NewText() => $"{Prefix}-{Guid.NewGuid():N}";
}
