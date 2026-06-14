using Microsoft.EntityFrameworkCore;
using Oip.Data.Repositories;

namespace Oip.Applications.Base.Data;

/// <summary>
/// Repository for application registry items.
/// </summary>
public class ApplicationRegistryRepository(ApplicationRegistryDbContext context)
    : BaseRepository<ApplicationRegistryItemEntity, long>(context)
{
    /// <summary>
    /// Retrieves an application registry item by code.
    /// </summary>
    public Task<ApplicationRegistryItemEntity?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(code);
        return DbSet.FirstOrDefaultAsync(
            x => x.Code.ToLower() == normalizedCode,
            cancellationToken);
    }

    /// <summary>
    /// Deletes an application registry item by code.
    /// </summary>
    public async Task DeleteByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await GetByCodeAsync(code, cancellationToken);
        if (entity is null)
            return;

        DbSet.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves pending changes.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToLowerInvariant();
    }
}
