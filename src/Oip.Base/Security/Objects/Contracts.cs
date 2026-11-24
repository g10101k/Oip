namespace Oip.Base.Security.Objects;

/// <summary>
/// Reference to a securable object: its registered type and its identifier within the type.
/// </summary>
/// <param name="Type">Code of the object type, for example acme.factory.</param>
/// <param name="Id">Identifier of the object.</param>
public readonly record struct ObjectRef(string Type, long Id)
{
    /// <summary>
    /// Largest identifier that a JavaScript number keeps without loss, the upper bound accepted by the API.
    /// </summary>
    public const long MaxClientSafeId = 9007199254740991;

    /// <inheritdoc />
    public override string ToString() => $"{Type}:{Id}";
}

/// <summary>
/// Entity that keeps the identifier of its effective access control set, so a list of such entities is filtered by
/// a single indexed condition without joining the access control tables of the object.
/// </summary>
public interface ISecurableEntity
{
    /// <summary>
    /// Identifier of the effective access control set. Maintained by the access control, never set it by hand.
    /// </summary>
    long AclSetId { get; set; }
}

/// <summary>
/// Rights given to one subject, expressed with right codes.
/// </summary>
/// <param name="SubjectKey">Subject key, see <see cref="SubjectKeys" />.</param>
/// <param name="Allow">Granted rights.</param>
/// <param name="Deny">Denied rights.</param>
public sealed record AclEntry(string SubjectKey, IReadOnlyCollection<string> Allow, IReadOnlyCollection<string>? Deny = null);
