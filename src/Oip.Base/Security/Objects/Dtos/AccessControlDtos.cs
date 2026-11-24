using System.ComponentModel.DataAnnotations;

namespace Oip.Base.Security.Objects.Dtos;

/// <summary>
/// Securable object type and its rights.
/// </summary>
public class AccessObjectTypeDto
{
    /// <summary>
    /// Code of the type.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Rights of the type.
    /// </summary>
    public List<AccessRightDto> Rights { get; set; } = [];
}

/// <summary>
/// Right of an object type.
/// </summary>
public class AccessRightDto
{
    /// <summary>
    /// Code of the right.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Display name, null for the standard rights that the client translates.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Subject that rights are given to.
/// </summary>
public class SubjectDto
{
    /// <summary>
    /// Subject key, for example role:engineers or user:{sub}.
    /// </summary>
    public string SubjectKey { get; set; } = null!;

    /// <summary>
    /// Type of the subject: role, user, client, group, everyone or owner.
    /// </summary>
    public string SubjectType { get; set; } = null!;

    /// <summary>
    /// Name to show.
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// False when the subject no longer exists in the identity provider.
    /// </summary>
    public bool IsKnown { get; set; } = true;
}

/// <summary>
/// Rights of one subject in an access control list.
/// </summary>
public class AclEntryDto : SubjectDto
{
    /// <summary>
    /// Granted rights.
    /// </summary>
    public List<string> Allow { get; set; } = [];

    /// <summary>
    /// Denied rights.
    /// </summary>
    public List<string> Deny { get; set; } = [];
}

/// <summary>
/// Access control list of an object.
/// </summary>
public class ObjectAclDto
{
    /// <summary>
    /// Code of the object type.
    /// </summary>
    public string ObjectType { get; set; } = null!;

    /// <summary>
    /// Identifier of the object.
    /// </summary>
    public long ObjectId { get; set; }

    /// <summary>
    /// Type of the parent the rights are inherited from.
    /// </summary>
    public string? ParentType { get; set; }

    /// <summary>
    /// Identifier of the parent the rights are inherited from.
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// When true the object does not inherit the rights of its parent.
    /// </summary>
    public bool InheritanceBroken { get; set; }

    /// <summary>
    /// Rights of the object type.
    /// </summary>
    public List<AccessRightDto> Rights { get; set; } = [];

    /// <summary>
    /// Rights assigned to the object explicitly.
    /// </summary>
    public List<AclEntryDto> Entries { get; set; } = [];

    /// <summary>
    /// Rights inherited from the parent. Empty when the inheritance is broken.
    /// </summary>
    public List<AclEntryDto> InheritedEntries { get; set; } = [];
}

/// <summary>
/// Rights of one subject to store.
/// </summary>
public class AclEntryUpdateDto
{
    /// <summary>
    /// Subject key.
    /// </summary>
    [Required]
    public string SubjectKey { get; set; } = null!;

    /// <summary>
    /// Granted rights.
    /// </summary>
    public List<string> Allow { get; set; } = [];

    /// <summary>
    /// Denied rights.
    /// </summary>
    public List<string> Deny { get; set; } = [];
}

/// <summary>
/// Request to replace the explicit rights of an object.
/// </summary>
public class UpdateObjectAclRequest
{
    /// <summary>
    /// Code of the object type.
    /// </summary>
    [Required]
    public string ObjectType { get; set; } = null!;

    /// <summary>
    /// Identifier of the object.
    /// </summary>
    [Range(1, ObjectRef.MaxClientSafeId)]
    public long ObjectId { get; set; }

    /// <summary>
    /// When true the object stops inheriting the rights of its parent.
    /// </summary>
    public bool InheritanceBroken { get; set; }

    /// <summary>
    /// Explicit rights of the object.
    /// </summary>
    public List<AclEntryUpdateDto> Entries { get; set; } = [];
}
