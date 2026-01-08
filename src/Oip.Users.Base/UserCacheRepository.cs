using System.Collections.Concurrent;
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

public class UserCacheRepository(GrpcUserService.GrpcUserServiceClient client, ILogger<UserCacheRepository> logger)
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

        var response = await client.GetAllUsersAsync(new GetAllUsersRequest()
        {
            PageSize = 1000, PageToken = string.Empty
        }, cancellationToken: cancellationToken);

        foreach (var user in response.Users)
        {
            Users.AddOrUpdate(user.UserId, user, (key, oldValue) => user);
        }
    }

    private Action<CancellationToken> SubscribeAction =>
        async void (t) =>
        {
            while (!t.IsCancellationRequested)
            {
                try
                {
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
}