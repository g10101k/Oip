namespace Oip.Base.Security.Objects.Data;

/// <summary>
/// Immutable set of access control entries shared by every object with the same effective or explicit rights.
/// </summary>
public class AclSetEntity
{
    /// <summary>
    /// Identifier of the set.
    /// </summary>
    public long AclSetId { get; set; }

    /// <summary>
    /// SHA-256 of the normalized entries, used to reuse an existing set instead of creating a duplicate.
    /// </summary>
    public string Hash { get; set; } = null!;

    /// <summary>
    /// Entries of the set.
    /// </summary>
    public List<AclSetEntryEntity> Entries { get; set; } = [];
}

/// <summary>
/// Rights granted or denied to one subject within an access control set.
/// </summary>
public class AclSetEntryEntity
{
    /// <summary>
    /// Identifier of the set the entry belongs to.
    /// </summary>
    public long AclSetId { get; set; }

    /// <summary>
    /// Subject key, for example role:engineers, user:{sub}, client:{client_id}, everyone or owner.
    /// </summary>
    public string SubjectKey { get; set; } = null!;

    /// <summary>
    /// Bit mask of the granted standard rights: read, edit, delete and manage-acl.
    /// </summary>
    public int AllowMask { get; set; }

    /// <summary>
    /// Bit mask of the denied standard rights. A denied right wins over a granted one.
    /// </summary>
    public int DenyMask { get; set; }

    /// <summary>
    /// Custom rights granted or denied to the subject.
    /// </summary>
    public List<AclSetEntryRightEntity> Rights { get; set; } = [];
}

/// <summary>
/// Custom right granted or denied to one subject within an access control set. The standard rights are kept in the
/// masks of <see cref="AclSetEntryEntity" />, any other right is kept as a row, so a context has no limit of rights.
/// </summary>
public class AclSetEntryRightEntity
{
    /// <summary>
    /// Identifier of the set the entry belongs to.
    /// </summary>
    public long AclSetId { get; set; }

    /// <summary>
    /// Subject key of the entry.
    /// </summary>
    public string SubjectKey { get; set; } = null!;

    /// <summary>
    /// Identifier of the custom right.
    /// </summary>
    public int RightId { get; set; }

    /// <summary>
    /// When true the right is denied, otherwise granted. A denied right wins over a granted one.
    /// </summary>
    public bool IsDeny { get; set; }
}

/// <summary>
/// Securable object known to the access control: its place in the hierarchy and its access control sets.
/// </summary>
public class AccessObjectEntity
{
    /// <summary>
    /// Identifier of the object type.
    /// </summary>
    public int ObjectTypeId { get; set; }

    /// <summary>
    /// Identifier of the object within its type.
    /// </summary>
    public long ObjectId { get; set; }

    /// <summary>
    /// Type of the parent object the rights are inherited from.
    /// </summary>
    public int? ParentTypeId { get; set; }

    /// <summary>
    /// Identifier of the parent object the rights are inherited from.
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// Set with the rights assigned to the object explicitly.
    /// </summary>
    public long? OwnAclSetId { get; set; }

    /// <summary>
    /// When true the object does not inherit the rights of its parent.
    /// </summary>
    public bool InheritanceBroken { get; set; }

    /// <summary>
    /// Set with the effective rights: the inherited rights merged with the explicit ones.
    /// </summary>
    public long AclSetId { get; set; }

    /// <summary>
    /// Subject of the user that owns the object.
    /// </summary>
    public string? OwnerSubject { get; set; }
}

/// <summary>
/// Registered type of securable objects.
/// </summary>
public class AccessObjectTypeEntity
{
    /// <summary>
    /// Identifier of the type.
    /// </summary>
    public int ObjectTypeId { get; set; }

    /// <summary>
    /// Unique code of the type, for example acme.factory.
    /// </summary>
    public string Code { get; set; } = null!;
}

/// <summary>
/// Custom right of a service context. A code has one identifier in the whole context, so an inherited set means the
/// same right in a parent and in a child of another type.
/// </summary>
public class AccessRightEntity
{
    /// <summary>
    /// Identifier of the right.
    /// </summary>
    public int RightId { get; set; }

    /// <summary>
    /// Unique code of the right, for example approve.
    /// </summary>
    public string Code { get; set; } = null!;
}

/// <summary>
/// Custom right available for an object type.
/// </summary>
public class AccessObjectTypeRightEntity
{
    /// <summary>
    /// Identifier of the object type.
    /// </summary>
    public int ObjectTypeId { get; set; }

    /// <summary>
    /// Identifier of the right.
    /// </summary>
    public int RightId { get; set; }
}
