using Microsoft.EntityFrameworkCore;

namespace Oip.Base.Security.Objects.Data;

/// <summary>
/// Database context of a service that keeps the access control of its own objects. The context calls
/// <see cref="AccessControlModelBuilderExtensions.ApplyOipAccessControl" /> in <c>OnModelCreating</c>.
/// </summary>
public interface IAccessControlDbContext
{
    /// <summary>
    /// Access control sets.
    /// </summary>
    DbSet<AclSetEntity> AclSets { get; }

    /// <summary>
    /// Entries of the access control sets.
    /// </summary>
    DbSet<AclSetEntryEntity> AclSetEntries { get; }

    /// <summary>
    /// Securable objects.
    /// </summary>
    DbSet<AccessObjectEntity> AccessObjects { get; }

    /// <summary>
    /// Registered object types.
    /// </summary>
    DbSet<AccessObjectTypeEntity> AccessObjectTypes { get; }

    /// <summary>
    /// Bits assigned to the rights of the object types.
    /// </summary>
    DbSet<AccessRightEntity> AccessRights { get; }
}
