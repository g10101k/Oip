using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oip.Base.Security.Objects.Data;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Object types handled by one service context.
/// </summary>
public interface IAccessControlRegistration
{
    /// <summary>
    /// Service context that keeps the objects.
    /// </summary>
    Type ContextType { get; }

    /// <summary>
    /// Registered object types.
    /// </summary>
    IReadOnlyList<SecurableObjectDefinition> Definitions { get; }

    /// <summary>
    /// Finds the definition of a type, or null when the type belongs to another context.
    /// </summary>
    SecurableObjectDefinition? Find(string type);
}

/// <summary>
/// Resolved object type: its database identifier and its rights. The standard rights are bits of the entry masks,
/// the custom rights are rows identified by <see cref="AccessRightEntity.RightId" />.
/// </summary>
public sealed class ResolvedObjectType
{
    internal ResolvedObjectType(SecurableObjectDefinition definition, int objectTypeId,
        IReadOnlyDictionary<string, int> customRightIds, IReadOnlyList<string> rightCodes)
    {
        Definition = definition;
        ObjectTypeId = objectTypeId;
        CustomRightIds = customRightIds;
        RightCodes = rightCodes;
    }

    /// <summary>
    /// Definition of the type.
    /// </summary>
    public SecurableObjectDefinition Definition { get; }

    /// <summary>
    /// Database identifier of the type.
    /// </summary>
    public int ObjectTypeId { get; }

    /// <summary>
    /// Identifier of every custom right of the type.
    /// </summary>
    public IReadOnlyDictionary<string, int> CustomRightIds { get; }

    /// <summary>
    /// Codes of every right of the type: the standard ones, the configured ones, then the ones added at run time.
    /// </summary>
    public IReadOnlyList<string> RightCodes { get; }

    /// <summary>
    /// Whether the right is defined for the type.
    /// </summary>
    public bool Knows(string right) => StandardRights.MaskOf(right) != 0 || CustomRightIds.ContainsKey(right);

    /// <summary>
    /// Mask of the standard rights and identifiers of the custom rights among the given rights.
    /// </summary>
    /// <exception cref="ArgumentException">A right is not defined for the type.</exception>
    internal (int Mask, IReadOnlyList<int> CustomIds) Encode(IEnumerable<string> rights)
    {
        var mask = 0;
        var customIds = new List<int>();
        foreach (var right in rights)
        {
            var bit = StandardRights.MaskOf(right);
            if (bit != 0)
                mask |= bit;
            else if (CustomRightIds.TryGetValue(right, out var id))
                customIds.Add(id);
            else
                throw new ArgumentException($"Right '{right}' is not defined for '{Definition.Type}'.", nameof(rights));
        }

        return (mask, customIds);
    }

    /// <summary>
    /// Right codes of a standard mask and custom right identifiers, in the order the rights are defined. Custom
    /// rights that the type does not have are skipped.
    /// </summary>
    internal List<string> RightsOf(int mask, IReadOnlySet<int> customIds) => RightCodes
        .Where(code => (StandardRights.MaskOf(code) & mask) != 0 ||
                       (CustomRightIds.TryGetValue(code, out var id) && customIds.Contains(id)))
        .ToList();
}

/// <summary>
/// Standard rights of every object type, kept as bits of the entry masks.
/// </summary>
public static class StandardRights
{
    /// <summary>
    /// Standard right codes, the position of a code is its bit.
    /// </summary>
    public static IReadOnlyList<string> Codes => AccessControlRegistryConstants.ReservedRights;

    /// <summary>
    /// Mask with every standard right.
    /// </summary>
    public const int All = 0b1111;

    /// <summary>
    /// Mask of a standard right, 0 for a custom right.
    /// </summary>
    public static int MaskOf(string right)
    {
        for (var i = 0; i < Codes.Count; i++)
            if (Codes[i] == right)
                return 1 << i;
        return 0;
    }
}

/// <summary>
/// Object types of a service context, with the identifiers of the types and of the custom rights stored in the
/// database. A custom right code has one identifier in the whole context, assigned once and never reused.
/// </summary>
public sealed class AccessControlRegistry<TContext> : IAccessControlRegistration
    where TContext : DbContext, IAccessControlDbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger? _logger;
    private readonly Dictionary<string, SecurableObjectDefinition> _byType;
    private readonly Dictionary<Type, SecurableObjectDefinition> _byEntity;
    private readonly Lock _lock = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, HashSet<string>> _addedRights = new();
    private volatile Resolved? _resolved;

    /// <summary>
    /// .ctor
    /// </summary>
    public AccessControlRegistry(AccessControlOptions<TContext> options, IServiceScopeFactory scopeFactory,
        ILogger<AccessControlRegistry<TContext>>? logger = null)
    {
        options.Validate();
        _scopeFactory = scopeFactory;
        _logger = logger;
        Definitions = options.Definitions;
        _byType = Definitions.ToDictionary(d => d.Type);
        _byEntity = Definitions.ToDictionary(d => d.EntityType);
    }

    /// <inheritdoc />
    public Type ContextType => typeof(TContext);

    /// <inheritdoc />
    public IReadOnlyList<SecurableObjectDefinition> Definitions { get; }

    /// <inheritdoc />
    public SecurableObjectDefinition? Find(string type) => _byType.GetValueOrDefault(type);

    /// <summary>
    /// Finds the definition of an entity class.
    /// </summary>
    public SecurableObjectDefinition? FindByEntity(Type entityType) => _byEntity.GetValueOrDefault(entityType);

    /// <summary>
    /// Returns the resolved type.
    /// </summary>
    /// <exception cref="ArgumentException">The type is not registered in this context.</exception>
    public ResolvedObjectType Get(string type)
    {
        var resolved = EnsureInitialized();
        return resolved.ByType.TryGetValue(type, out var result)
            ? result
            : throw new ArgumentException($"Object type '{type}' is not registered in {typeof(TContext).Name}.",
                nameof(type));
    }

    /// <summary>
    /// Returns the resolved type that knows the right. For a type with stored rights, a right added by another
    /// process after this one started is loaded from the database.
    /// </summary>
    public ResolvedObjectType Get(string type, string right)
    {
        var resolved = Get(type);
        if (resolved.Knows(right) || !resolved.Definition.AcceptsStoredRights)
            return resolved;

        Reload();
        return Get(type);
    }

    /// <summary>
    /// Adds rights to a type with stored rights, for example the rights a module declares. Each new right gets an
    /// identifier that stays fixed, like a right configured in code.
    /// </summary>
    /// <exception cref="InvalidOperationException">The type does not accept stored rights.</exception>
    public void EnsureRights(string type, IEnumerable<string> codes)
    {
        var definition = Find(type) ?? throw new ArgumentException(
            $"Object type '{type}' is not registered in {typeof(TContext).Name}.", nameof(type));
        if (!definition.AcceptsStoredRights)
            throw new InvalidOperationException($"Object type '{type}' takes its rights from code only.");

        var added = _addedRights.GetOrAdd(type, _ => []);
        bool changed;
        lock (added)
            changed = codes.Where(code => !string.IsNullOrWhiteSpace(code)).Aggregate(false,
                (any, code) => added.Add(code) | any);

        if (changed && Get(type) is var resolved && added.Any(code => !resolved.Knows(code)))
            Reload();
    }

    /// <summary>
    /// Synchronizes the types and rights with the database again.
    /// </summary>
    public void Reload()
    {
        lock (_lock)
            _resolved = SynchronizeWithRetry();
    }

    /// <summary>
    /// Returns the resolved type by its database identifier.
    /// </summary>
    public ResolvedObjectType Get(int objectTypeId) => EnsureInitialized().ById[objectTypeId];

    /// <summary>
    /// Synchronizes the types and rights with the database once per process.
    /// </summary>
    public void Initialize() => EnsureInitialized();

    private Resolved EnsureInitialized()
    {
        if (_resolved is not null)
            return _resolved;

        lock (_lock)
        {
            if (_resolved is not null)
                return _resolved;

            _resolved = SynchronizeWithRetry();
            return _resolved;
        }
    }

    private Resolved SynchronizeWithRetry()
    {
        try
        {
            return Synchronize();
        }
        catch (DbUpdateException)
        {
            // Another instance registered the same types or rights at the same moment; the stored values win.
            return Synchronize();
        }
    }

    private IEnumerable<string> CustomRightsOf(SecurableObjectDefinition definition)
    {
        var configured = definition.Rights.Select(r => r.Code)
            .Where(code => !AccessControlRegistryConstants.ReservedRights.Contains(code));
        if (!_addedRights.TryGetValue(definition.Type, out var added))
            return configured;
        lock (added)
            return configured.Concat(added.Where(code => !AccessControlRegistryConstants.ReservedRights.Contains(code)))
                .Distinct().ToList();
    }

    private Resolved Synchronize()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        var types = context.AccessObjectTypes.ToList();
        foreach (var definition in Definitions.Where(d => types.All(t => t.Code != d.Type)))
        {
            var type = new AccessObjectTypeEntity { Code = definition.Type };
            context.AccessObjectTypes.Add(type);
            types.Add(type);
        }

        context.SaveChanges();

        // A custom right code has one identifier in the whole context: an inherited set carries the rights of the
        // parent type, so a custom right of the parent must mean the same right, or nothing, in the child.
        var wanted = Definitions.ToDictionary(d => d.Type, d => CustomRightsOf(d).ToList());
        var rights = context.AccessRights.ToList();
        foreach (var code in wanted.Values.SelectMany(codes => codes).Distinct()
                     .Where(code => rights.All(r => r.Code != code)))
        {
            var right = new AccessRightEntity { Code = code };
            context.AccessRights.Add(right);
            rights.Add(right);
        }

        context.SaveChanges();

        var links = context.AccessObjectTypeRights.ToList();
        foreach (var definition in Definitions)
        {
            var typeId = types.Single(t => t.Code == definition.Type).ObjectTypeId;
            foreach (var code in wanted[definition.Type])
            {
                var rightId = rights.Single(r => r.Code == code).RightId;
                if (links.Any(l => l.ObjectTypeId == typeId && l.RightId == rightId))
                    continue;
                var link = new AccessObjectTypeRightEntity { ObjectTypeId = typeId, RightId = rightId };
                context.AccessObjectTypeRights.Add(link);
                links.Add(link);
            }
        }

        context.SaveChanges();

        var codeById = rights.ToDictionary(r => r.RightId, r => r.Code);
        var resolvedTypes = Definitions.Select(definition =>
        {
            var typeId = types.Single(t => t.Code == definition.Type).ObjectTypeId;
            var configured = definition.Rights.Select(r => r.Code).Concat(wanted[definition.Type]).Distinct().ToList();
            var custom = links.Where(l => l.ObjectTypeId == typeId && codeById.ContainsKey(l.RightId))
                .Select(l => (Code: codeById[l.RightId], l.RightId))
                .Where(r => definition.AcceptsStoredRights || configured.Contains(r.Code))
                .ToDictionary(r => r.Code, r => r.RightId);
            var codes = configured.Where(code => StandardRights.MaskOf(code) != 0 || custom.ContainsKey(code))
                .Concat(custom.Where(r => !configured.Contains(r.Key)).OrderBy(r => r.Value).Select(r => r.Key))
                .ToList();
            return new ResolvedObjectType(definition, typeId, custom, codes);
        }).ToList();

        WarnAboutOwnerEntries(context, types);

        return new Resolved(resolvedTypes.ToDictionary(t => t.Definition.Type),
            resolvedTypes.ToDictionary(t => t.ObjectTypeId));
    }

    /// <summary>
    /// An entry of the owner subject grants nothing on an object whose type has no owner. It is still useful on a
    /// parent for its owned children, so only types with no owner in their whole subtree of types are reported.
    /// </summary>
    private void WarnAboutOwnerEntries(TContext context, IReadOnlyList<AccessObjectTypeEntity> types)
    {
        if (_logger is null)
            return;

        var candidates = Definitions.Where(d => !d.HasOwner && !HasOwnedDescendant(d)).Select(d => d.Type).ToList();
        if (candidates.Count == 0)
            return;

        var typeIds = types.Where(t => candidates.Contains(t.Code)).Select(t => t.ObjectTypeId).ToList();
        var entries = context.AclSetEntries;
        var affected = context.AccessObjects
            .Where(o => typeIds.Contains(o.ObjectTypeId) &&
                        entries.Any(e => e.AclSetId == o.AclSetId && e.SubjectKey == SubjectKeys.Owner))
            .Select(o => o.ObjectTypeId)
            .Distinct()
            .ToList();
        if (affected.Count == 0)
            return;

        _logger.LogWarning(
            "Objects of {Types} in {Context} have entries of the '{Owner}' subject, but neither these types nor " +
            "their child types have an owner, so the entries grant nothing. Configure the owner with Owner(...).",
            string.Join(", ", types.Where(t => affected.Contains(t.ObjectTypeId)).Select(t => t.Code)),
            typeof(TContext).Name, SubjectKeys.Owner);
    }

    private bool HasOwnedDescendant(SecurableObjectDefinition definition) =>
        Definitions.Any(d => d.ParentType == definition.Type && d != definition &&
                             (d.HasOwner || HasOwnedDescendant(d)));

    private sealed record Resolved(
        IReadOnlyDictionary<string, ResolvedObjectType> ByType,
        IReadOnlyDictionary<int, ResolvedObjectType> ById);
}

/// <summary>
/// Synchronizes the object types of a context on startup, so the first request does not pay for it.
/// </summary>
public sealed class AccessControlStartupTask<TContext>(AccessControlRegistry<TContext> registry)
    : Runtime.IStartupTask where TContext : DbContext, IAccessControlDbContext
{
    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        registry.Initialize();
        return Task.CompletedTask;
    }
}
