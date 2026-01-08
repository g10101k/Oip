using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Oip.Data.Repositories;

/// <summary>
/// Base repository with common CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key</typeparam>
/// <param name="context">The database context</param>
public abstract class BaseRepository<TEntity, TKey>(DbContext context) where TEntity : class
{
    /// <summary>
    /// Represents the set of entities in the database for a given type
    /// </summary>
    protected DbSet<TEntity> DbSet => context.Set<TEntity>();

    /// <summary>
    /// Retrieves an entity by its primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found; otherwise, null</returns>
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id!], cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of all entities</returns>
    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">The condition to filter entities</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of matching entities</returns>
    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the database
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes an entity by its primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Checks if any entity matches the specified predicate
    /// </summary>
    /// <param name="predicate">The condition to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any matching entity exists; otherwise, false</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Counts entities, optionally filtered by a predicate
    /// </summary>
    /// <param name="predicate">The condition to filter entities; null to count all</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of matching entities</returns>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await DbSet.CountAsync(cancellationToken);
        return await DbSet.CountAsync(predicate, cancellationToken);
    }
}