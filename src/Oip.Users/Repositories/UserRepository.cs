using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Dtos;
using Oip.Users.Contexts;
using Oip.Users.Entities;

namespace Oip.Users.Repositories;

/// <summary>
/// Provides data access operations for user entities.
/// </summary>
public class UserRepository
{
    private readonly UserContext _context;

    /// <summary>
    /// Provides data access operations for user entities.
    /// </summary>
    public UserRepository(UserContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Asynchronously retrieves a user entity by its unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user entity if found; otherwise, null.</returns>
    public async Task<UserEntity?> GetByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    /// <summary>
    /// Asynchronously retrieves a user entity by its Keycloak identifier.
    /// </summary>
    /// <param name="keycloakId">The Keycloak identifier of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user entity if found; otherwise, null.</returns>
    public async Task<UserEntity?> GetByKeycloakIdAsync(string keycloakId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
    }

    /// <summary>
    /// Asynchronously retrieves all user entities with support for pagination.
    /// </summary>
    /// <param name="skip">The number of elements to skip before returning the remaining elements.</param>
    /// <param name="take">The number of elements to return.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user entities.</returns>
    public async Task<IEnumerable<UserEntity>> GetAllAsync(int skip = 0, int take = 100)
    {
        return await _context.Users
            .OrderBy(u => u.UserId)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Asynchronously searches for user entities based on a search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for in email, first name, or last name.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user entities matching the search term.</returns>
    public async Task<IEnumerable<UserEntity>> SearchAsync(string searchTerm)
    {
        return await _context.Users
            .Where(u => u.Email.Contains(searchTerm) ||
                        u.FirstName.Contains(searchTerm) ||
                        u.LastName.Contains(searchTerm))
            .Take(50)
            .ToListAsync();
    }

    /// <summary>
    /// Asynchronously adds a new user entity to the data store.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added user entity.</returns>
    public async Task<UserEntity> AddAsync(UserEntity user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Asynchronously updates an existing user entity in the data store.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated user entity.</returns>
    public async Task<UserEntity> UpdateAsync(UserEntity user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Asynchronously deletes a user entity by its unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteAsync(int userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Asynchronously checks if a user entity exists by its Keycloak identifier.
    /// </summary>
    /// <param name="keycloakId">The Keycloak identifier of the user to check for existence.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the user exists; otherwise, false.</returns>
    public async Task<bool> ExistsByKeycloakIdAsync(string keycloakId)
    {
        return await _context.Users
            .AnyAsync(u => u.KeycloakId == keycloakId);
    }

    /// <summary>
    /// Asynchronously retrieves the total count of user entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total count of user entities.</returns>
    public async Task<int> CountAsync()
    {
        return await _context.Users.CountAsync();
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