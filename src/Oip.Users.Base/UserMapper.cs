using Google.Protobuf;
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
    [MapperIgnoreTarget(nameof(UserDto.Phone))]
    public static partial UserDto ToDto(this UserEntity user);

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
            Photo: user.Photo.ToByteArray(),
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
            Photo = user.Photo is null ? ByteString.Empty : ByteString.CopyFrom(user.Photo),
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
