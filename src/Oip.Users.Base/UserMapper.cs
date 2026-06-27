using Google.Protobuf.WellKnownTypes;
using Oip.Base.Services;
using Oip.Users.Base.Data.Entities;
using Riok.Mapperly.Abstractions;

namespace Oip.Users.Base;

/// <summary>
/// Maps user contracts between entities, DTOs, cache DTOs, and gRPC shapes.
/// </summary>
[Mapper]
public static partial class UserMapper
{
    public static UserDto ToDto(this UserEntity user)
    {
        return new UserDto(
            UserId: user.UserId,
            KeycloakId: user.KeycloakId,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            LastSyncedAt: user.LastSyncedAt,
            Settings: user.Settings);
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            UserId: user.UserId,
            KeycloakId: user.KeycloakId,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt.ToDateTimeOffset(),
            UpdatedAt: user.UpdatedAt.ToDateTimeOffset(),
            LastSyncedAt: user.LastSyncedAt?.ToDateTimeOffset(),
            Settings: user.Settings,
            Phone: user.Phone);
    }

    public static User ToGrpc(this UserEntity user)
    {
        return new User
        {
            UserId = user.UserId,
            KeycloakId = user.KeycloakId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt.ToTimestamp(),
            UpdatedAt = user.UpdatedAt.ToTimestamp(),
            LastSyncedAt = user.LastSyncedAt.ToTimestamp(),
            Settings = user.Settings
        };
    }

    public static UserCacheDto ToCacheDto(this UserDto user)
    {
        return new UserCacheDto(
            user.UserId,
            user.KeycloakId,
            user.Email,
            user.Phone);
    }

    public static UserCacheDto ToCacheDto(this User user)
    {
        return new UserCacheDto(
            user.UserId,
            user.KeycloakId,
            user.Email,
            user.Phone);
    }

}
