using System.Collections.Concurrent;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Oip.Base.Extensions;
using Oip.Base.Services;
using Oip.Users.Base;

namespace Oip.Users.Base.Data.Repositories;

/// <summary>
/// A hosted service that periodically collects and evaluates tags using a specified interval
/// </summary>
public class UserCacheRepositoryHostedService(IServiceScopeFactory scopeFactory, ILogger<UserCacheRepository> logger)
    : PeriodicBackgroundService<UserCacheRepository>(scopeFactory, logger);

public class UserCacheRepository(
    IServiceScopeFactory scopeFactory,
    IServiceProvider serviceProvider,
    ILogger<UserCacheRepository> logger)
    : IPeriodicalService, IUserCacheRepository
{
    private readonly ConcurrentDictionary<int, UserCacheDto> _users = new();

    public IReadOnlyDictionary<int, UserCacheDto> Users => _users;

    public int Interval => 360;

    private bool SubscribeActionStarted { get; set; }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!SubscribeActionStarted)
        {
            SubscribeActionStarted = true;
            SubscribeAction.Invoke(cancellationToken);
        }

        await scopeFactory.ExecuteAsync<IUserService>(async (userService) =>
        {
            try
            {
                var response = await userService.GetAllUsersAsync(1, 1000, cancellationToken);

                foreach (var user in response.Users)
                {
                    var cachedUser = user.ToCacheDto();
                    _users.AddOrUpdate(cachedUser.UserId, cachedUser, (key, oldValue) => cachedUser);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled exception.");
            }
        });
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
        var cachedUser = eventMessage.User.ToCacheDto();
        _users.AddOrUpdate(cachedUser.UserId, cachedUser, (i, user) => user);
        logger.LogDebug("{json}", JsonConvert.SerializeObject(eventMessage));
    }

    public UserCacheDto? GetUserByKeycloakUserId(string key)
    {
        return _users.Values.FirstOrDefault(x => x.KeycloakId == key);
    }
}
