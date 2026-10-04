namespace Oip.UiTest.Pages;

/// <summary>
/// The discussion of an object: comments with edit history, mentions, reactions and attachments.
/// Comments are found by a unique part of their text.
/// </summary>
internal class DiscussionPage(IWebDriver driver) : BasePage(driver)
{
    /// <summary>
    /// The built-in discussion page. It shows the discussion of object 1 of type 1.
    /// </summary>
    public const string Path = "/discussion/1";

    private static readonly By NewComment = By.CssSelector("[qa-id='oip-discussion-new-comment']");
    private static readonly By AttachInput = By.CssSelector("[qa-id='oip-discussion-attach-input']");
    private static readonly By PendingFile = By.CssSelector("[qa-id='oip-discussion-pending-file']");
    private static readonly By SendButton = By.CssSelector("[qa-id='oip-discussion-send'] button");
    private static readonly By MentionSuggestion = By.CssSelector("[qa-id='oip-discussion-mention-suggestion']");
    private static readonly By EditButton = By.CssSelector("[qa-id='oip-discussion-comment-edit']");
    private static readonly By EditContent = By.CssSelector("[qa-id='oip-discussion-edit-content']");
    private static readonly By SaveEditButton = By.CssSelector("[qa-id='oip-discussion-edit-save'] button");
    private static readonly By EditedTag = By.CssSelector("[qa-id='oip-discussion-comment-edited']");
    private static readonly By HistoryButton = By.CssSelector("[qa-id='oip-discussion-comment-history']");
    private static readonly By HistoryOld = By.CssSelector("[qa-id='oip-discussion-history-old']");
    private static readonly By HistoryNew = By.CssSelector("[qa-id='oip-discussion-history-new']");
    private static readonly By DeleteButton = By.CssSelector("[qa-id='oip-discussion-comment-delete']");
    private static readonly By Mention = By.CssSelector("[qa-id='oip-discussion-comment-content'] .mention-token");
    private static readonly By Reaction = By.CssSelector("[qa-id='oip-discussion-reaction']");
    private static readonly By AddReactionButton = By.CssSelector("[qa-id='oip-discussion-add-reaction']");
    private static readonly By Emoji = By.CssSelector("[qa-id='oip-discussion-emoji']");
    private static readonly By AttachmentName = By.CssSelector("[qa-id='oip-discussion-attachment-name']");
    private static readonly By AttachmentDelete = By.CssSelector("[qa-id='oip-discussion-attachment-delete']");

    /// <summary>
    /// Opens the discussion page and waits until the comments have loaded.
    /// </summary>
    public DiscussionPage Open()
    {
        Navigate(Path);
        WaitForAppInteractive();
        Wait.UntilFindElement(NewComment);
        Wait.Until(_ => !ExistsNow(By.CssSelector("discussion p-progressspinner")));
        return this;
    }

    /// <summary>
    /// Whether a comment containing the text is shown.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public bool Contains(string text) => ExistsNow(Comment(text));

    /// <summary>
    /// Posts a comment and waits until it is shown.
    /// </summary>
    /// <param name="text">Comment text.</param>
    /// <param name="attachmentPath">File to attach as the browser sees it, or null.</param>
    public void Post(string text, string? attachmentPath = null)
    {
        Type(text);
        if (attachmentPath is not null)
        {
            Driver.FindElement(AttachInput).SendKeys(attachmentPath);
            Wait.UntilFindElement(PendingFile);
        }

        Wait.UntilClick(SendButton);
        Wait.UntilFindElement(Comment(text));
    }

    /// <summary>
    /// Types the text and a mention of the user, picks the user from the suggestions and posts the comment.
    /// Returns the posted text, which carries the e-mail of the mentioned user.
    /// </summary>
    /// <param name="text">Comment text before the mention.</param>
    /// <param name="username">User name to search the suggestions with.</param>
    public string PostWithMention(string text, string username)
    {
        Type($"{text} @{username}");
        Wait.UntilClick(By.XPath(
            $"//*[@qa-id='oip-discussion-mention-suggestion'][contains(normalize-space(), '{username}')]"));
        Wait.UntilDisappear(MentionSuggestion);
        var posted = Driver.FindElement(NewComment).GetAttribute("value")!.Trim();

        Wait.UntilClick(SendButton);
        Wait.UntilFindElement(Comment(text));
        return posted;
    }

    /// <summary>
    /// Mentions rendered in the comment, e.g. <c>@user@user.ru</c>.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public IReadOnlyList<string> GetMentions(string text) =>
        FindAllNow(CommentElement(text), Mention).Select(mention => mention.Text.Trim()).ToList();

    /// <summary>
    /// Replaces the comment text and waits until the new text is shown.
    /// </summary>
    /// <param name="text">Unique part of the current comment text.</param>
    /// <param name="newText">New comment text.</param>
    public void Edit(string text, string newText)
    {
        CommentElement(text).FindElement(EditButton).Click();
        var input = Wait.UntilFindElement(EditContent);
        input.Clear();
        input.SendKeys(newText);
        Wait.UntilClick(SaveEditButton);
        Wait.UntilFindElement(Comment(newText));
    }

    /// <summary>
    /// Whether the comment is marked as edited.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public bool IsEdited(string text) => FindAllNow(CommentElement(text), EditedTag).Count != 0;

    /// <summary>
    /// Opens the edit history of the comment and returns the text before and after the first edit.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public (string Before, string After) GetFirstEdit(string text)
    {
        CommentElement(text).FindElement(HistoryButton).Click();
        var before = Wait.Until(_ => CommentElement(text).FindElement(HistoryOld)).Text.Trim();
        var after = CommentElement(text).FindElement(HistoryNew).Text.Trim();
        return (before, after);
    }

    /// <summary>
    /// Reacts to the comment with the first emoji of the palette and returns the reaction caption,
    /// e.g. <c>👍 1</c>.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public string React(string text)
    {
        CommentElement(text).FindElement(AddReactionButton).Click();
        // The emoji popover is appended to the body.
        Wait.UntilClick(Emoji);
        return Wait.Until(_ => CommentElement(text).FindElement(Reaction)).Text.Trim();
    }

    /// <summary>
    /// Reactions of the comment as their captions, e.g. <c>👍 1</c>.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public IReadOnlyList<string> GetReactions(string text) =>
        FindAllNow(CommentElement(text), Reaction).Select(reaction => reaction.Text.Trim()).ToList();

    /// <summary>
    /// Clicks the reaction of the current user to take it back and waits until it is gone.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public void Unreact(string text)
    {
        CommentElement(text).FindElement(Reaction).Click();
        Wait.Until(_ => GetReactions(text).Count == 0);
    }

    /// <summary>
    /// File names of the comment attachments.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public IReadOnlyList<string> GetAttachments(string text) =>
        FindAllNow(CommentElement(text), AttachmentName).Select(name => name.Text.Trim()).ToList();

    /// <summary>
    /// Deletes the only attachment of the comment, confirming it, and waits until it is gone.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public void DeleteAttachment(string text)
    {
        CommentElement(text).FindElement(AttachmentDelete).Click();
        new ConfirmDialog(Driver).Accept();
        Wait.Until(_ => GetAttachments(text).Count == 0);
    }

    /// <summary>
    /// Deletes the comment, confirming it, and waits until it is gone.
    /// </summary>
    /// <param name="text">Unique part of the comment text.</param>
    public void Delete(string text)
    {
        CommentElement(text).FindElement(DeleteButton).Click();
        new ConfirmDialog(Driver).Accept();
        Wait.UntilDisappear(Comment(text));
    }

    private void Type(string text)
    {
        var input = Wait.UntilFindElement(NewComment);
        input.Clear();
        input.SendKeys(text);
    }

    private IWebElement CommentElement(string text) => Wait.UntilFindElement(Comment(text));

    private static By Comment(string text) =>
        By.XPath("//*[@qa-id='oip-discussion-comment']" +
                 $"[.//*[@qa-id='oip-discussion-comment-content'][contains(normalize-space(), '{text}')]]");
}
