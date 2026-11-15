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
/// Resolved object type: its database identifier and the bits of its rights.
/// </summary>
public sealed class ResolvedObjectType
{
    internal ResolvedObjectType(SecurableObjectDefinition definition, int objectTypeId,
        IReadOnlyDictionary<string, byte> bits)
    {
        Definition = definition;
        ObjectTypeId = objectTypeId;
        Bits = bits;
        AllRightsMask = bits.Values.Aggregate(0L, (mask, bit) => mask | (1L << bit));
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
    /// Bit of every right of the type.
    /// </summary>
    public IReadOnlyDictionary<string, byte> Bits { get; }

    /// <summary>
    /// Mask with every right of the type.
    /// </summary>
    public long AllRightsMask { get; }

    /// <summary>
    /// Mask of one right.
    /// </summary>
    /// <exception cref="ArgumentException">The right is not defined for the type.</exception>
    public long MaskOf(string right)
    {
        if (!Bits.TryGetValue(right, out var bit))
            throw new ArgumentException($"Right '{right}' is not defined for '{Definition.Type}'.", nameof(right));
        return 1L << bit;
    }

    /// <summary>
    /// Mask of several rights.
    /// </summary>
    public long MaskOf(IEnumerable<string> rights) => rights.Aggregate(0L, (mask, right) => mask | MaskOf(right));

    /// <summary>
    /// Right codes of a mask, in the order the rights are defined.
    /// </summary>
    public List<string> RightsOf(long mask) => Definition.Rights.Select(r => r.Code)
        .Where(code => Bits.TryGetValue(code, out var bit) && (mask & (1L << bit)) != 0).ToList();
}

/// <summary>
/// Object types of a service context, with the identifiers and right bits stored in the database. The bits are
/// assigned once and stay fixed, so a stored mask keeps its meaning across releases.
/// </summary>
public sealed class AccessControlRegistry<TContext> : IAccessControlRegistration
    where TContext : DbContext, IAccessControlDbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger? _logger;
    private readonly Dictionary<string, SecurableObjectDefinition> _byType;
    private readonly Dictionary<Type, SecurableObjectDefinition> _byEntity;
    private readonly Lock _lock = new();
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
    /// Returns the resolved type by its database identifier.
    /// </summary>
    public ResolvedObjectType Get(int objectTypeId) => EnsureInitialized().ById[objectTypeId];

    /// <summary>
    /// Synchronizes the types and right bits with the database once per process.
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

            try
            {
                _resolved = Synchronize();
            }
            catch (DbUpdateException)
            {
                // Another instance registered the same types at the same moment; the stored values win.
                _resolved = Synchronize();
            }

            return _resolved;
        }
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

        var rights = context.AccessRights.ToList();
        foreach (var definition in Definitions)
        {
            var typeId = types.Single(t => t.Code == definition.Type).ObjectTypeId;
            var typeRights = rights.Where(r => r.ObjectTypeId == typeId).ToList();

            for (var i = 0; i < AccessControlRegistryConstants.ReservedRights.Count; i++)
            {
                var code = AccessControlRegistryConstants.ReservedRights[i];
                if (typeRights.Any(r => r.Code == code))
                    continue;
                if (typeRights.Any(r => r.Bit == i))
                    throw new InvalidOperationException(
                        $"Bit {i} of '{definition.Type}' is taken by another right, the standard right '{code}' cannot be stored.");
                var right = new AccessRightEntity { ObjectTypeId = typeId, Code = code, Bit = (byte)i };
                context.AccessRights.Add(right);
                typeRights.Add(right);
                rights.Add(right);
            }

            // A right code has the same bit in every type of the context: an inherited set carries the masks of the
            // parent type, so a custom right of the parent must mean the same right, or nothing, in the child.
            foreach (var custom in definition.Rights.Select(r => r.Code)
                         .Where(code => typeRights.All(r => r.Code != code)))
            {
                var shared = rights.FirstOrDefault(r => r.Code == custom);
                int bit;
                if (shared is not null)
                {
                    bit = shared.Bit;
                    if (typeRights.Any(r => r.Bit == bit))
                        throw new InvalidOperationException(
                            $"Bit {bit} of right '{custom}' is taken by another right of '{definition.Type}'.");
                }
                else
                {
                    var used = rights.Select(r => (int)r.Bit).ToHashSet();
                    bit = AccessControlRegistryConstants.ReservedRights.Count;
                    while (used.Contains(bit))
                        bit++;
                    if (bit > AccessControlRegistryConstants.MaxBit)
                        throw new InvalidOperationException(
                            $"{typeof(TContext).Name} has no free right bits left for '{custom}'.");
                }

                var right = new AccessRightEntity { ObjectTypeId = typeId, Code = custom, Bit = (byte)bit };
                context.AccessRights.Add(right);
                typeRights.Add(right);
                rights.Add(right);
            }
        }

        context.SaveChanges();

        var resolvedTypes = Definitions.Select(definition =>
        {
            var typeId = types.Single(t => t.Code == definition.Type).ObjectTypeId;
            var known = definition.Rights.Select(r => r.Code).ToHashSet();
            var bits = rights.Where(r => r.ObjectTypeId == typeId && known.Contains(r.Code))
                .ToDictionary(r => r.Code, r => r.Bit);
            return new ResolvedObjectType(definition, typeId, bits);
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
