using Microsoft.EntityFrameworkCore;
using Oip.Data.Contexts;
using Oip.Data.Dtos;
using Oip.Data.Entities;

namespace Oip.Data.Repositories;

/// <summary>
/// User repository
/// </summary>
public class UserRepository
{
    private readonly OipContext _context;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="context"></param>
    public UserRepository(OipContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public GetUserDto? GetUserByEmail(string email)
    {
        var user = _context.Users.Where(x => x.Email == email).AsNoTracking().FirstOrDefault();
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
        var user = _context.Users.FirstOrDefault(x => x.Email == email);
        if (user == null)
        {
            user = new UserEntity()
            {
                Email = email,
                Photo = photo
            };
            _context.Users.Add(user);
            
        }
        else
        {
            user.Photo = photo;
        }
        _context.SaveChanges();
    }
}