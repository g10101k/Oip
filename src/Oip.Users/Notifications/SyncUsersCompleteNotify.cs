namespace Oip.Users.Notifications;

/// <summary>Notification indicating the completion of a user synchronization process, including the number of users processed and the time frame</summary>
/// <param name="Count">The number of users that were synchronized</param>
/// <param name="Start">The starting time of the synchronization process</param>
/// <param name="End">The ending time of the synchronization process</param>
public record SyncUsersCompleteNotify(int Count, DateTimeOffset Start, DateTimeOffset End);
