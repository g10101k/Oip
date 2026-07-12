namespace Oip.Notifications.Base.Services;

/// <summary>
/// Calls the remote notifications gRPC service.
/// </summary>
public class GrpcNotificationServiceClientAdapter(GrpcNotificationService.GrpcNotificationServiceClient client)
    : INotificationServiceClient
{
    /// <inheritdoc />
    public async Task<CreateNotificationTypesResponse> CreateNotificationTypesAsync(
        CreateNotificationTypesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.CreateNotificationTypesAsync(request, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateNotificationAsync(CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        await client.CreateNotificationAsync(request, cancellationToken: cancellationToken);
    }
}