using System.Collections.Concurrent;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Oip.Base.Extensions;
using Oip.Users.Base;
using Oip.Users.Repositories;

namespace Oip.Users.Services;

/// <summary>
/// Service for managing user data including subscriptions to real-time changes and retrieval of user information
/// </summary>
public class UserService(IServiceScopeFactory scopeFactory, ILogger<UserService> logger)
    : GrpcUserService.GrpcUserServiceBase
{
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<UserChangeEvent>> Subscribers = new();

    /// <summary>
    /// Subscribes a client to real-time data streams for specified event types.
    /// </summary>
    /// <param name="request">The subscription request containing the client ID and event types.</param>
    /// <param name="responseStream">The stream to send event messages to the client.</param>
    /// <param name="context">The context for the gRPC call.</param>
    public override async Task SubscribeToUserChanges(SubscribeRequest request,
        IServerStreamWriter<UserChangeEvent> responseStream,
        ServerCallContext context)
    {
        var clientId = request.ClientId;

        Subscribers[clientId] = responseStream;
        logger.LogDebug(@"Client {ClientId} subscribed to events", clientId);

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, context.CancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogDebug(@"Client {ClientId} disconnected", clientId);
        }
        finally
        {
            Subscribers.TryRemove(clientId, out _);
        }
    }

    /// <summary>
    /// Retrieves tags based on the provided request.
    /// </summary>
    /// <param name="request">The request containing interface ID.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>A response containing the retrieved tags.</returns>
    public override Task<GetAllUsersResponse> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
    {
        return scopeFactory.ExecuteAsync<UserRepository, GetAllUsersResponse>(async repository =>
        {
            var pageSize = request.PageSize > 0 ? request.PageSize : 100; // Default page size
            var pageToken = request.PageToken;

            // Parse page token for pagination (assuming it's a page number)
            int pageNumber = 1;
            if (!string.IsNullOrEmpty(pageToken) && int.TryParse(pageToken, out int parsedPage))
            {
                pageNumber = parsedPage;
            }

            // Get users from repository with pagination
            var res = await repository.GetAllUsersAsync(pageNumber, pageSize);

            // Convert domain users to gRPC users
            var grpcUsers = res.Results.Select(user => new User
            {
                UserId = user.UserId,
                KeycloakId = user.KeycloakId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt.ToTimestamp(),
                UpdatedAt = user.UpdatedAt.ToTimestamp(),
                LastSyncedAt = user.LastSyncedAt.ToTimestamp() ?? null,
                Photo = user.Photo != null ? ByteString.CopyFrom(user.Photo) : ByteString.Empty,
                Settings = user.Settings
            }).ToList();

            // Calculate next page token
            var nextPageToken = string.Empty;
            if (pageNumber * pageSize < res.TotalPages)
            {
                nextPageToken = (pageNumber + 1).ToString();
            }

            var response = new GetAllUsersResponse
            {
                Users = { grpcUsers },
                NextPageToken = nextPageToken,
                TotalCount = res.TotalPages
            };

            logger.LogDebug("Retrieved {Count} users for page {PageNumber} (size {PageSize}), total: {TotalCount}",
                grpcUsers.Count, pageNumber, pageSize, res.TotalPages);

            return response;
        });
    }

    /// <summary>
    /// Publishes a user change event to all subscribed clients
    /// </summary>
    public async Task PublishUserEvent(UserChangeEvent eventMessage)
    {
        var subscribersSnapshot = Subscribers.ToArray();

        foreach (var subscriber in subscribersSnapshot)
        {
            try
            {
                await subscriber.Value.WriteAsync(eventMessage);
                logger.LogDebug(@"Event for userid {UserUserId} sent to client {SubscriberKey}",
                    eventMessage.User.UserId, subscriber.Key);
            }
            catch (Exception ex)
            {
                logger.LogDebug(@"Failed to send event to client {SubscriberKey}: {ExMessage}", subscriber.Key,
                    ex.Message);
                Subscribers.TryRemove(subscriber.Key, out _);
            }
        }
    }
}