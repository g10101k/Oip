using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Entities;

namespace Oip.Base.Data.Repositories;

/// <summary>
/// User repository
/// </summary>
public class UserRepository
{
    private readonly OipModuleContext _moduleContext;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="moduleContext"></param>
    public UserRepository(OipModuleContext moduleContext)
    {
        _moduleContext = moduleContext;
    }

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>A <see cref="GetUserDto"/> representing the user, or null if no user is found with the given email.</returns>
    public async Task<GetUserDto?> GetUserByEmail(string email)
    {
        var user = await _moduleContext.Users.Where(x => x.Email == email).AsNoTracking().FirstOrDefaultAsync();
        if (user == null)
            return null;
        else
            return new GetUserDto(user.UserId, user.Email, user.Photo);
    }

    /// <summary>
    /// Upserts a user's photo.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <param name="photo">The user's photo as a byte array.</param>
    public void UpsertUserPhoto(string email, byte[] photo)
    {
        var user = _moduleContext.Users.FirstOrDefault(x => x.Email == email);
        if (user == null)
        {
            user = new UserEntity()
            {
                Email = email,
                Photo = photo
            };
            _moduleContext.Users.Add(user);
            
        }
        else
        {
            user.Photo = photo;
        }
        _moduleContext.SaveChanges();
    }
}