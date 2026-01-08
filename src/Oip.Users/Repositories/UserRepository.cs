using Microsoft.EntityFrameworkCore;
using Oip.Data.Dtos;
using Oip.Data.Repositories;
using Oip.Users.Contexts;
using Oip.Users.Entities;

namespace Oip.Users.Repositories;

/// <summary>
/// Provides data access operations for user entities.
/// </summary>
public class UserRepository(UserContext context) : BaseRepository<UserEntity, int>(context)
{
    /// <summary>
    /// Asynchronously retrieves a user entity by its Keycloak identifier.
    /// </summary>
    /// <param name="keycloakId">The Keycloak identifier of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user entity if found; otherwise, null.</returns>
    public async Task<UserEntity?> GetByKeycloakIdAsync(string keycloakId)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
    }

    /// <summary>
    /// Gets all users with pagination support
    /// </summary>
    /// <param name="pageNumber">Page number (starting from 1)</param>
    /// <param name="pageSize">Number of users per page</param>
    /// <returns>UsersPagedResult with users list and total count</returns>
    public async Task<PageResult<UserEntity>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 100; // Default page size
        if (pageSize > 1000) pageSize = 1000; // Maximum page size

        var query = context.Users
            .Where(u => u.IsActive) // Filter by active users if needed
            .OrderBy(u => u.UserId); // Order by ID for consistent pagination

        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PageResult<UserEntity>(users, totalCount, pageNumber);
    }


    /// <summary>
    /// Asynchronously retrieves all user entities with support for pagination.
    /// </summary>
    /// <param name="skip">The number of elements to skip before returning the remaining elements.</param>
    /// <param name="take">The number of elements to return.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user entities.</returns>
    public async Task<IEnumerable<UserEntity>> GetAllAsync(int skip = 0, int take = 100)
    {
        return await context.Users
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
        return await context.Users
            .Where(u => u.Email.Contains(searchTerm) ||
                        u.FirstName.Contains(searchTerm) ||
                        u.LastName.Contains(searchTerm))
            .Take(50)
            .ToListAsync();
    }

    /// <summary>
    /// Asynchronously updates an existing user entity in the data store.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated user entity.</returns>
    public async Task<UserEntity> UpdateAsync(UserEntity user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
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
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Asynchronously checks if a user entity exists by its Keycloak identifier.
    /// </summary>
    /// <param name="keycloakId">The Keycloak identifier of the user to check for existence.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the user exists; otherwise, false.</returns>
    public async Task<bool> ExistsByKeycloakIdAsync(string keycloakId)
    {
        return await context.Users
            .AnyAsync(u => u.KeycloakId == keycloakId);
    }

    /// <summary>
    /// Asynchronously retrieves the total count of user entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total count of user entities.</returns>
    public async Task<int> CountAsync()
    {
        return await context.Users.CountAsync();
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public GetUserDto? GetUserByEmail(string email)
    {
        var user = context.Users.Where(x => x.Email == email).AsNoTracking().FirstOrDefault();
        if (user == null)
            return null;
        else
            return new GetUserDto(user.UserId, user.Email, user.Photo);
    }

    /// <summary>
    /// Get user settings
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public string GetUserSettings(string email)
    {
        return context.Users.Where(x => x.Email == email).AsNoTracking().FirstOrDefault()?.Settings ?? string.Empty;
    }

    /// <summary>
    /// Update user photo
    /// </summary>
    /// <param name="email"></param>
    /// <param name="photo"></param>
    public void UpsertUserPhoto(string email, byte[] photo)
    {
        var user = context.Users.FirstOrDefault(x => x.Email == email);
        if (user == null)
        {
            user = new UserEntity()
            {
                Email = email,
                Photo = photo
            };
            context.Users.Add(user);
        }
        else
        {
            user.Photo = photo;
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Update User settings
    /// </summary>
    /// <param name="email">email</param>
    /// <param name="json">settings</param>
    /// <exception cref="InvalidOperationException">User not found</exception>
    public async Task UpdateUserSettings(string email, string json)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email) ??
                   throw new InvalidOperationException($"User with email: {email} - not found");
        user.Settings = json;
        await context.SaveChangesAsync();
    }
}