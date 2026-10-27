namespace Oip.Users.Base.Controllers.Api;

/// <summary>
/// Number of the synchronized users by activity.
/// </summary>
/// <param name="TotalCount">Number of all users.</param>
/// <param name="ActiveCount">Number of active users.</param>
/// <param name="InactiveCount">Number of inactive users.</param>
public record UsersSummaryDto(int TotalCount, int ActiveCount, int InactiveCount);
