using System.Collections.Concurrent;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Oip.Base.Services;

namespace Oip.Users.Base;

/// <summary>
/// A hosted service that periodically collects and evaluates tags using a specified interval
/// </summary>
public class UserCacheRepositoryHostedService(IServiceScopeFactory scopeFactory, ILogger<UserCacheRepository> logger)
    : PeriodicBackgroundService<UserCacheRepository>(scopeFactory, logger);

public class UserCacheRepository(
    IUserService userService,
    IServiceProvider serviceProvider,
    ILogger<UserCacheRepository> logger)
    : IPeriodicalService
{
    public readonly ConcurrentDictionary<int, User> Users = new();
    public int Interval => 360;

    private bool SubscribeActionStarted { get; set; }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!SubscribeActionStarted)
        {
            SubscribeActionStarted = true;
            SubscribeAction.Invoke(cancellationToken);
        }

        var response = await userService.GetAllUsersAsync(1, 1000, cancellationToken);

        foreach (var user in response.Users)
        {
            var grpcUser = MapToGrpcUser(user);
            Users.AddOrUpdate(grpcUser.UserId, grpcUser, (key, oldValue) => grpcUser);
        }
    }

    private Action<CancellationToken> SubscribeAction =>
        async void (t) =>
        {
            while (!t.IsCancellationRequested)
            {
                try
                {
                    var client = serviceProvider.GetService<GrpcUserService.GrpcUserServiceClient>();
                    if (client == null)
                    {
                        return;
                    }

                    var clientId = $"client_{Guid.NewGuid().ToString()[..8]}";

                    logger.LogInformation("Connecting as {ClientId}...", clientId);
                    var request = new SubscribeRequest
                    {
                        ClientId = clientId,
                    };

                    using var call = client.SubscribeToUserChanges(request, cancellationToken: t);
                    var responseStream = call.ResponseStream;

                    await foreach (var eventMessage in responseStream.ReadAllAsync(cancellationToken: t))
                    {
                        ProcessEventMessage(eventMessage);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    logger.LogError(ex, "Subscription was cancelled.");
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    logger.LogError(ex, "Service is unavailable.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled exception.");
                }

                await Task.Delay(10000, t);
            }
        };

    private void ProcessEventMessage(UserChangeEvent eventMessage)
    {
        Users.AddOrUpdate(eventMessage.User.UserId, eventMessage.User, (i, user) => user);
        logger.LogDebug("{json}", JsonConvert.SerializeObject(eventMessage));
    }

    private static User MapToGrpcUser(UserDto user)
    {
        return new User
        {
            UserId = user.UserId,
            KeycloakId = user.KeycloakId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = Timestamp.FromDateTimeOffset(user.CreatedAt),
            UpdatedAt = Timestamp.FromDateTimeOffset(user.UpdatedAt),
            LastSyncedAt = user.LastSyncedAt.HasValue ? Timestamp.FromDateTimeOffset(user.LastSyncedAt.Value) : null,
            Photo = user.Photo != null ? Google.Protobuf.ByteString.CopyFrom(user.Photo) : Google.Protobuf.ByteString.Empty,
            Settings = user.Settings,
            Phone = user.Phone
        };
    }

    public User? GetUserByKeycloakUserId(string key)
    {
        return Users.Values.FirstOrDefault(x => x.KeycloakId == key);
    }
}
