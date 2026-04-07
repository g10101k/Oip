using Grpc.Core;
using Microsoft.Extensions.Logging;
using Oip.Users.Base;
using Google.Protobuf.WellKnownTypes;

namespace Oip.Users.Base;

/// <summary>
/// Remote implementation of IUserService that communicates via gRPC.
/// Used when IsStandalone is false.
/// </summary>
public class RemoteUserService(GrpcUserService.GrpcUserServiceClient client, ILogger<RemoteUserService> logger)
    : IUserService
{
    public async Task<UserPagedResult> GetAllUsersAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var request = new GetAllUsersRequest
        {
            PageSize = pageSize,
            PageToken = (pageNumber - 1).ToString() // Simplified token logic
        };

        try
        {
            var response = await client.GetAllUsersAsync(request, cancellationToken: cancellationToken);

            var users = response.Users.Select(MapFromGrpc).ToList();

            return new UserPagedResult(
                Users: users,
                TotalCount: response.TotalCount,
                PageNumber: pageNumber,
                PageSize: pageSize
            );
        }
        catch (RpcException ex)
        {
            logger.LogError(ex, "Error calling GetAllUsers via gRPC");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var users = await GetAllUsersSnapshotAsync(cancellationToken);
        return users.FirstOrDefault(x => x.UserId == userId);
    }

    public async Task<UserDto?> GetUserByKeycloakIdAsync(string keycloakId,
        CancellationToken cancellationToken = default)
    {
        var users = await GetAllUsersSnapshotAsync(cancellationToken);
        return users.FirstOrDefault(x => string.Equals(x.KeycloakId, keycloakId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var users = await GetAllUsersSnapshotAsync(cancellationToken);
        return users.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersByIdsAsync(
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToHashSet();
        if (ids.Count == 0)
        {
            return [];
        }

        var users = await GetAllUsersSnapshotAsync(cancellationToken);
        return users.Where(x => ids.Contains(x.UserId)).ToList();
    }

    public async Task<IReadOnlyList<UserDto>> SearchUsersAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var normalized = searchTerm.Trim();
        var users = await GetAllUsersSnapshotAsync(cancellationToken);
        return users
            .Where(x =>
                Contains(x.Email, normalized) ||
                Contains(x.FirstName, normalized) ||
                Contains(x.LastName, normalized) ||
                Contains($"{x.FirstName} {x.LastName}".Trim(), normalized))
            .Take(50)
            .ToList();
    }

    private static UserDto MapFromGrpc(User grpcUser)
    {
        return new UserDto(
            UserId: grpcUser.UserId,
            KeycloakId: grpcUser.KeycloakId,
            Email: grpcUser.Email,
            FirstName: grpcUser.FirstName,
            LastName: grpcUser.LastName,
            IsActive: grpcUser.IsActive,
            CreatedAt: grpcUser.CreatedAt.ToDateTimeOffset(),
            UpdatedAt: grpcUser.UpdatedAt.ToDateTimeOffset(),
            LastSyncedAt: grpcUser.LastSyncedAt?.ToDateTimeOffset(),
            Photo: grpcUser.Photo.ToByteArray(),
            Settings: grpcUser.Settings,
            Phone: grpcUser.Phone
        );
    }

    private async Task<List<UserDto>> GetAllUsersSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var results = new List<UserDto>();
            var pageToken = "1";
            const int pageSize = 500;

            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await client.GetAllUsersAsync(new GetAllUsersRequest
                {
                    PageSize = pageSize,
                    PageToken = pageToken
                }, cancellationToken: cancellationToken);

                results.AddRange(response.Users.Select(MapFromGrpc));
                if (string.IsNullOrWhiteSpace(response.NextPageToken))
                {
                    break;
                }

                pageToken = response.NextPageToken;
            }

            return results;
        }
        catch (RpcException ex)
        {
            logger.LogError(ex, "Error fetching users snapshot via gRPC");
            throw;
        }
    }

    private static bool Contains(string? value, string searchTerm)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}