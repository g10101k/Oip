using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Constants;
using Oip.Base.Security.Objects.Data;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Securable object types of one service context.
/// </summary>
/// <typeparam name="TContext">Service context that keeps the objects and their access control.</typeparam>
public sealed class AccessControlOptions<TContext> where TContext : DbContext, IAccessControlDbContext
{
    private readonly List<SecurableObjectDefinition> _definitions = [];

    /// <summary>
    /// Registered object types.
    /// </summary>
    public IReadOnlyList<SecurableObjectDefinition> Definitions => _definitions;

    /// <summary>
    /// Registers an entity as a securable object type.
    /// </summary>
    /// <param name="type">Unique code of the type, prefixed by the application, for example acme.factory.</param>
    /// <param name="configure">Configures the identifier, the parent and the rights of the type.</param>
    public AccessControlOptions<TContext> Object<TEntity>(string type, Action<SecurableObjectBuilder<TEntity>> configure)
        where TEntity : class
    {
        var builder = new SecurableObjectBuilder<TEntity>(type);
        configure(builder);
        _definitions.Add(builder.Build());
        return this;
    }

    internal void Validate()
    {
        var duplicates = _definitions.GroupBy(d => d.Type).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
            throw new InvalidOperationException($"Object types are registered twice: {string.Join(", ", duplicates)}.");

        var byType = _definitions.ToDictionary(d => d.Type);
        foreach (var definition in _definitions)
        {
            if (definition.ParentType is null)
                continue;
            if (!byType.ContainsKey(definition.ParentType))
                throw new InvalidOperationException(
                    $"Parent type '{definition.ParentType}' of '{definition.Type}' is not registered in " +
                    $"{typeof(TContext).Name}. A parent must belong to the same service context.");

            var visited = new HashSet<string> { definition.Type };
            var current = byType[definition.ParentType];
            while (current.ParentType is not null && current.ParentType != current.Type)
            {
                if (!visited.Add(current.Type))
                    throw new InvalidOperationException($"Object type '{definition.Type}' has a cyclic parent chain.");
                current = byType[current.ParentType];
            }
        }
    }
}

/// <summary>
/// Configures a securable object type.
/// </summary>
public sealed class SecurableObjectBuilder<TEntity> where TEntity : class
{
    private readonly string _type;
    private readonly List<AccessRightDefinition> _rights = [];
    private readonly List<AclEntry> _defaultAcl = [];
    private Expression<Func<TEntity, long>>? _id;
    private string? _parentType;
    private Expression<Func<TEntity, long?>>? _parentId;
    private Expression<Func<TEntity, string?>>? _owner;
    private bool _fillOwnerOnInsert;

    internal SecurableObjectBuilder(string type)
    {
        if (string.IsNullOrWhiteSpace(type) || type.Length > AccessControlModelBuilderExtensions.CodeMaxLength)
            throw new ArgumentException("Object type code is empty or too long.", nameof(type));
        _type = type;
    }

    /// <summary>
    /// Sets the identifier of the object. An int or long key of the entity.
    /// </summary>
    public SecurableObjectBuilder<TEntity> Id(Expression<Func<TEntity, long>> id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the parent the object inherits its rights from. The parent type is registered in the same context.
    /// </summary>
    /// <param name="parentType">Code of the parent type.</param>
    /// <param name="parentId">Identifier of the parent, null for a root object.</param>
    public SecurableObjectBuilder<TEntity> Parent(string parentType, Expression<Func<TEntity, long?>> parentId)
    {
        _parentType = parentType;
        _parentId = parentId;
        return this;
    }

    /// <summary>
    /// Sets the owner of the object, the user for whom the entries of the <see cref="SubjectKeys.Owner" /> subject
    /// apply. The owner is a Keycloak subject (sub) kept in any string property of the entity.
    /// </summary>
    /// <param name="owner">Property with the subject of the owner.</param>
    /// <param name="fillOnInsert">When true, an empty owner is filled with the subject of the creator on insert.
    /// The property must then be writable.</param>
    public SecurableObjectBuilder<TEntity> Owner(Expression<Func<TEntity, string?>> owner, bool fillOnInsert = false)
    {
        if (fillOnInsert && owner.Body is not MemberExpression { Member: PropertyInfo { CanWrite: true } })
            throw new ArgumentException(
                $"Owner of '{_type}' is filled on insert, so it must be a writable property of the entity.",
                nameof(owner));
        _owner = owner;
        _fillOwnerOnInsert = fillOnInsert;
        return this;
    }

    /// <summary>
    /// Adds rights besides read, edit, delete and manage-acl.
    /// </summary>
    public SecurableObjectBuilder<TEntity> Rights(params string[] codes)
    {
        foreach (var code in codes)
            Right(code);
        return this;
    }

    /// <summary>
    /// Adds a right with a display name.
    /// </summary>
    public SecurableObjectBuilder<TEntity> Right(string code, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > AccessControlModelBuilderExtensions.CodeMaxLength)
            throw new ArgumentException("Right code is empty or too long.", nameof(code));
        if (AccessControlRegistryConstants.ReservedRights.Contains(code) || _rights.Any(r => r.Code == code))
            throw new ArgumentException($"Right '{code}' of '{_type}' is already defined.", nameof(code));
        _rights.Add(new AccessRightDefinition(code, name));
        return this;
    }

    /// <summary>
    /// Explicit rights given to a new root object, one without a parent.
    /// </summary>
    public SecurableObjectBuilder<TEntity> DefaultAcl(params AclEntry[] entries)
    {
        _defaultAcl.AddRange(entries);
        return this;
    }

    internal SecurableObjectDefinition Build()
    {
        if (_id is null)
            throw new InvalidOperationException($"Identifier of the object type '{_type}' is not configured.");
        return new SecurableObjectDefinition<TEntity>(_type, _id, _parentType, _parentId, _owner, _fillOwnerOnInsert,
            _rights, _defaultAcl);
    }
}

/// <summary>
/// Right of an object type.
/// </summary>
/// <param name="Code">Code of the right.</param>
/// <param name="Name">Display name, null for the standard rights translated by the client.</param>
public sealed record AccessRightDefinition(string Code, string? Name);

/// <summary>
/// Registered securable object type.
/// </summary>
public abstract class SecurableObjectDefinition
{
    /// <summary>
    /// Code of the type.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Entity class of the type.
    /// </summary>
    public abstract Type EntityType { get; }

    /// <summary>
    /// Code of the parent type, or null for a root type.
    /// </summary>
    public string? ParentType { get; }

    /// <summary>
    /// Rights of the type: the standard ones followed by the custom ones.
    /// </summary>
    public IReadOnlyList<AccessRightDefinition> Rights { get; }

    /// <summary>
    /// Explicit rights given to a new root object.
    /// </summary>
    public IReadOnlyList<AclEntry> DefaultAcl { get; }

    /// <summary>
    /// Whether the type has an owner, see <see cref="SecurableObjectBuilder{TEntity}.Owner" />.
    /// </summary>
    public abstract bool HasOwner { get; }

    /// <summary>
    /// Whether an empty owner is filled with the creator on insert.
    /// </summary>
    public abstract bool FillOwnerOnInsert { get; }

    /// <summary>
    /// .ctor
    /// </summary>
    protected SecurableObjectDefinition(string type, string? parentType, IEnumerable<AccessRightDefinition> customRights,
        IReadOnlyList<AclEntry> defaultAcl)
    {
        Type = type;
        ParentType = parentType;
        Rights = AccessControlRegistryConstants.ReservedRights.Select(code => new AccessRightDefinition(code, null))
            .Concat(customRights).ToList();
        DefaultAcl = defaultAcl;
    }

    internal abstract long GetId(object entity);

    internal abstract long? GetParentId(object entity);

    internal abstract string? GetOwner(object entity);

    internal abstract void SetOwner(object entity, string? owner);

    internal ObjectRef? GetParent(object entity)
    {
        if (ParentType is null)
            return null;
        var parentId = GetParentId(entity);
        return parentId is null or 0 ? null : new ObjectRef(ParentType, parentId.Value);
    }

    internal abstract Task UpdateAclSetIdsAsync(DbContext context, IReadOnlyDictionary<long, long> aclSetIds,
        CancellationToken cancellationToken);
}

internal sealed class SecurableObjectDefinition<TEntity> : SecurableObjectDefinition where TEntity : class
{
    private const int BatchSize = 500;
    private readonly Expression<Func<TEntity, long>> _idExpression;
    private readonly Func<TEntity, long> _id;
    private readonly Func<TEntity, long?>? _parentId;
    private readonly Func<TEntity, string?>? _owner;
    private readonly PropertyInfo? _ownerProperty;
    private readonly bool _fillOwnerOnInsert;

    public SecurableObjectDefinition(string type, Expression<Func<TEntity, long>> id, string? parentType,
        Expression<Func<TEntity, long?>>? parentId, Expression<Func<TEntity, string?>>? owner, bool fillOwnerOnInsert,
        IEnumerable<AccessRightDefinition> customRights, IReadOnlyList<AclEntry> defaultAcl)
        : base(type, parentType, customRights, defaultAcl)
    {
        _idExpression = id;
        _id = id.Compile();
        _parentId = parentId?.Compile();
        OwnerExpression = owner;
        _owner = owner?.Compile();
        _ownerProperty = (owner?.Body as MemberExpression)?.Member as PropertyInfo;
        _fillOwnerOnInsert = fillOwnerOnInsert;
    }

    public override Type EntityType => typeof(TEntity);

    public override bool HasOwner => OwnerExpression is not null;

    public override bool FillOwnerOnInsert => _fillOwnerOnInsert;

    /// <summary>
    /// Owner of the entity as a query expression, null for a type without an owner.
    /// </summary>
    internal Expression<Func<TEntity, string?>>? OwnerExpression { get; }

    internal override long GetId(object entity) => _id((TEntity)entity);

    internal override long? GetParentId(object entity) => _parentId?.Invoke((TEntity)entity);

    internal override string? GetOwner(object entity) => _owner?.Invoke((TEntity)entity);

    internal override void SetOwner(object entity, string? owner)
    {
        if (_ownerProperty is null || !_ownerProperty.CanWrite)
            throw new InvalidOperationException($"Owner of '{Type}' is not a writable property.");
        _ownerProperty.SetValue(entity, owner);
    }

    internal override async Task UpdateAclSetIdsAsync(DbContext context, IReadOnlyDictionary<long, long> aclSetIds,
        CancellationToken cancellationToken)
    {
        if (!typeof(ISecurableEntity).IsAssignableFrom(typeof(TEntity)) || aclSetIds.Count == 0)
            return;

        foreach (var batch in aclSetIds.Keys.Chunk(BatchSize))
        {
            var entities = await context.Set<TEntity>().Where(IdIn(batch)).ToListAsync(cancellationToken);
            foreach (var entity in entities)
                ((ISecurableEntity)entity).AclSetId = aclSetIds[_id(entity)];
        }
    }

    /// <summary>
    /// Identifier of the entity as a query expression.
    /// </summary>
    internal Expression<Func<TEntity, long>> IdExpression => _idExpression;

    /// <summary>
    /// Predicate that keeps the entities with the given identifiers.
    /// </summary>
    internal Expression<Func<TEntity, bool>> IdIn(IEnumerable<long> ids)
    {
        var holder = new IdHolder { Ids = ids.ToList() };
        var contains = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [typeof(long)],
            Expression.Property(Expression.Constant(holder), nameof(IdHolder.Ids)), _idExpression.Body);
        return Expression.Lambda<Func<TEntity, bool>>(contains, _idExpression.Parameters);
    }

    private sealed class IdHolder
    {
        public List<long> Ids { get; init; } = [];
    }
}

internal static class AccessControlRegistryConstants
{
    public static readonly IReadOnlyList<string> ReservedRights =
        [SecurityConstants.Read, SecurityConstants.Edit, SecurityConstants.Delete, SecurityConstants.ManageAcl];

    public const int MaxBit = 62;
}
