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
    /// Get user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public GetUserDto? GetUserByEmail(string email)
    {
        var user = _moduleContext.Users.Where(x => x.Email == email).AsNoTracking().FirstOrDefault();
        if (user == null)
            return null;
        else
            return new GetUserDto(user.UserId, user.Email, user.Photo);
    }

    /// <summary>
    /// Update user photo
    /// </summary>
    /// <param name="email"></param>
    /// <param name="photo"></param>
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