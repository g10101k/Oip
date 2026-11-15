using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Oip.Base.Data.Constants;
using Oip.Base.Exceptions;
using Oip.Base.Security.Objects.Data;
using Oip.Base.Security.Objects.Dtos;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Checks and changes the rights on securable objects of any registered context.
/// </summary>
public interface IObjectAccessService
{
    /// <summary>
    /// Registered object types of every context.
    /// </summary>
    IReadOnlyList<AccessObjectTypeDto> GetObjectTypes();

    /// <summary>
    /// Whether the type is registered.
    /// </summary>
    bool IsRegistered(string objectType);

    /// <summary>
    /// Whether the caller holds the right on the object. False for an object unknown to the access control.
    /// </summary>
    Task<bool> HasRightAsync(ObjectRef obj, string right, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rights of the caller on the object.
    /// </summary>
    Task<IReadOnlyList<string>> GetEffectiveRightsAsync(ObjectRef obj, CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicit and inherited rights of the object. Requires the manage-acl right.
    /// </summary>
    Task<ObjectAclDto> GetAclAsync(ObjectRef obj, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the explicit rights of the object and updates its descendants. Requires the manage-acl right.
    /// </summary>
    Task SetAclAsync(ObjectRef obj, bool inheritanceBroken, IReadOnlyCollection<AclEntry> entries,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Access control of the objects kept in one service context.
/// </summary>
public sealed class ObjectAccessService<TContext>(
    TContext context,
    AccessControlRegistry<TContext> registry,
    ISubjectResolver subjectResolver) : IObjectAccessService
    where TContext : DbContext, IAccessControlDbContext
{
    private readonly Dictionary<long, Dictionary<string, Masks>> _setCache = [];

    /// <summary>
    /// Registry of the context.
    /// </summary>
    public AccessControlRegistry<TContext> Registry => registry;

    /// <inheritdoc />
    public IReadOnlyList<AccessObjectTypeDto> GetObjectTypes() => registry.Definitions.Select(ToDto).ToList();

    /// <inheritdoc />
    public bool IsRegistered(string objectType) => registry.Find(objectType) is not null;

    /// <inheritdoc />
    public async Task<bool> HasRightAsync(ObjectRef obj, string right, CancellationToken cancellationToken = default)
    {
        var type = registry.Get(obj.Type);
        var mask = type.MaskOf(right);
        return (await GetEffectiveMaskAsync(type, obj.Id, subjectResolver.Resolve(), cancellationToken) & mask) != 0;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetEffectiveRightsAsync(ObjectRef obj,
        CancellationToken cancellationToken = default)
    {
        var type = registry.Get(obj.Type);
        return type.RightsOf(await GetEffectiveMaskAsync(type, obj.Id, subjectResolver.Resolve(), cancellationToken));
    }

    /// <summary>
    /// Keeps only the entities the caller holds the right on.
    /// </summary>
    public IQueryable<T> WhereAccessible<T>(IQueryable<T> query, string right) where T : class, ISecurableEntity
    {
        var definition = registry.FindByEntity(typeof(T))
                         ?? throw new ArgumentException(
                             $"{typeof(T).Name} is not registered as a securable object in {typeof(TContext).Name}.");
        var mask = registry.Get(definition.Type).MaskOf(right);
        var subjects = subjectResolver.Resolve();

        if (subjects.IsAdmin)
            return query;
        if (!subjects.IsAuthenticated)
            return query.Where(_ => false);

        if (definition is SecurableObjectDefinition<T> { OwnerExpression: { } ownerOf } &&
            subjects.UserSubject is not null)
            return FilterOwned(query, mask, subjects, ownerOf);

        return Filter(query, mask, subjects);
    }

    /// <summary>
    /// Rights of the caller on each of the given entities, for example the rows of a page. Runs one query per right,
    /// not per entity, and reads the identifier as configured with <see cref="SecurableObjectBuilder{TEntity}.Id" />.
    /// </summary>
    /// <param name="ids">Identifiers of the entities.</param>
    /// <param name="rights">Rights to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Granted rights by identifier, with an empty list for an entity without any of them.</returns>
    public async Task<Dictionary<long, List<string>>> GetRightsByIdsAsync<T>(IReadOnlyCollection<long> ids,
        IReadOnlyCollection<string> rights, CancellationToken cancellationToken = default)
        where T : class, ISecurableEntity
    {
        var definition = registry.FindByEntity(typeof(T)) as SecurableObjectDefinition<T>
                         ?? throw new ArgumentException(
                             $"{typeof(T).Name} is not registered as a securable object in {typeof(TContext).Name}.");

        var result = ids.Distinct().ToDictionary(id => id, _ => new List<string>());
        if (result.Count == 0)
            return result;

        foreach (var right in rights)
        {
            var granted = await WhereAccessible(context.Set<T>().AsNoTracking(), right)
                .Where(definition.IdIn(result.Keys))
                .Select(definition.IdExpression)
                .ToListAsync(cancellationToken);
            foreach (var id in granted)
                result[id].Add(right);
        }

        return result;
    }

    private IQueryable<T> Filter<T>(IQueryable<T> query, long mask, SubjectSet subjects)
        where T : class, ISecurableEntity
    {
        var keys = subjects.Keys.ToList();
        var entries = context.AclSetEntries;
        return query.Where(e =>
            entries.Any(a => a.AclSetId == e.AclSetId && keys.Contains(a.SubjectKey) && (a.AllowMask & mask) != 0) &&
            !entries.Any(a => a.AclSetId == e.AclSetId && keys.Contains(a.SubjectKey) && (a.DenyMask & mask) != 0));
    }

    private IQueryable<T> FilterOwned<T>(IQueryable<T> query, long mask, SubjectSet subjects,
        Expression<Func<T, string?>> ownerOf)
        where T : class, ISecurableEntity
    {
        var keys = subjects.Keys.ToList();
        var subject = subjects.UserSubject;
        const string owner = SubjectKeys.Owner;
        var entries = context.AclSetEntries;

        // The owner value is a placeholder that is replaced with the owner expression of the type below.
        Expression<Func<T, string?, bool>> template = (e, ownerValue) =>
            entries.Any(a => a.AclSetId == e.AclSetId &&
                             (keys.Contains(a.SubjectKey) || (a.SubjectKey == owner && ownerValue == subject)) &&
                             (a.AllowMask & mask) != 0) &&
            !entries.Any(a => a.AclSetId == e.AclSetId &&
                              (keys.Contains(a.SubjectKey) || (a.SubjectKey == owner && ownerValue == subject)) &&
                              (a.DenyMask & mask) != 0);

        var entity = template.Parameters[0];
        var ownerBody = new ParameterReplacer(ownerOf.Parameters[0], entity).Visit(ownerOf.Body);
        var body = new ParameterReplacer(template.Parameters[1], ownerBody).Visit(template.Body);
        return query.Where(Expression.Lambda<Func<T, bool>>(body, entity));
    }

    private sealed class ParameterReplacer(ParameterExpression parameter, Expression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == parameter ? replacement : base.VisitParameter(node);
    }

    /// <inheritdoc />
    public async Task<ObjectAclDto> GetAclAsync(ObjectRef obj, CancellationToken cancellationToken = default)
    {
        var type = registry.Get(obj.Type);
        var accessObject = await FindAsync(type, obj.Id, cancellationToken) ?? throw NotRegistered(obj);
        await DemandManageAclAsync(type, accessObject, cancellationToken);

        var result = new ObjectAclDto
        {
            ObjectType = obj.Type,
            ObjectId = obj.Id,
            InheritanceBroken = accessObject.InheritanceBroken,
            Rights = ToDto(type.Definition).Rights,
            Entries = accessObject.OwnAclSetId is { } ownId
                ? ToEntries(type, await LoadSetAsync(ownId, cancellationToken))
                : []
        };

        if (accessObject.ParentTypeId is { } parentTypeId && accessObject.ParentId is { } parentId)
        {
            var parentType = registry.Get(parentTypeId);
            result.ParentType = parentType.Definition.Type;
            result.ParentId = parentId;

            var parent = await FindAsync(parentType, parentId, cancellationToken);
            if (parent is not null && !accessObject.InheritanceBroken)
                result.InheritedEntries = ToEntries(type, await LoadSetAsync(parent.AclSetId, cancellationToken));
        }

        return result;
    }

    /// <inheritdoc />
    public Task SetAclAsync(ObjectRef obj, bool inheritanceBroken, IReadOnlyCollection<AclEntry> entries,
        CancellationToken cancellationToken = default) =>
        SetAclCoreAsync(obj, inheritanceBroken, entries, true, cancellationToken);

    /// <summary>
    /// Replaces the explicit rights of the object without checking the rights of the caller. For seeding, migrations
    /// and background jobs only.
    /// </summary>
    public Task SetAclUncheckedAsync(ObjectRef obj, bool inheritanceBroken, IReadOnlyCollection<AclEntry> entries,
        CancellationToken cancellationToken = default) =>
        SetAclCoreAsync(obj, inheritanceBroken, entries, false, cancellationToken);

    private async Task SetAclCoreAsync(ObjectRef obj, bool inheritanceBroken, IReadOnlyCollection<AclEntry> entries,
        bool demandManageAcl, CancellationToken cancellationToken)
    {
        var type = registry.Get(obj.Type);
        var accessObject = await FindAsync(type, obj.Id, cancellationToken) ?? throw NotRegistered(obj);
        if (demandManageAcl)
            await DemandManageAclAsync(type, accessObject, cancellationToken);
        var masks = ToMasks(type, entries);

        await InTransactionAsync(async () =>
        {
            accessObject.OwnAclSetId = masks.Count == 0 ? null : await GetOrCreateSetAsync(masks, cancellationToken);
            accessObject.InheritanceBroken = inheritanceBroken;
            await RecomputeAsync(accessObject, cancellationToken);
        }, cancellationToken);
    }

    /// <summary>
    /// Moves the object to another parent and updates the rights of its subtree.
    /// </summary>
    public async Task MoveAsync(ObjectRef obj, ObjectRef? newParent, CancellationToken cancellationToken = default)
    {
        var type = registry.Get(obj.Type);
        var accessObject = await FindAsync(type, obj.Id, cancellationToken) ?? throw NotRegistered(obj);
        await InTransactionAsync(() => MoveCoreAsync(type, accessObject, newParent, cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Registers a new object, or returns the effective set of an object that is already registered.
    /// </summary>
    /// <returns>Identifier of the effective access control set of the object.</returns>
    public async Task<long> RegisterAsync(ObjectRef obj, ObjectRef? parent, string? ownerSubject,
        CancellationToken cancellationToken = default)
    {
        var type = registry.Get(obj.Type);
        var existing = await FindAsync(type, obj.Id, cancellationToken);
        if (existing is not null)
            return existing.AclSetId;

        var accessObject = new AccessObjectEntity
        {
            ObjectTypeId = type.ObjectTypeId,
            ObjectId = obj.Id,
            OwnerSubject = ownerSubject
        };

        if (parent is { } parentRef)
        {
            var parentType = ValidateParent(type, parentRef);
            accessObject.ParentTypeId = parentType.ObjectTypeId;
            accessObject.ParentId = parentRef.Id;
        }
        else if (type.Definition.DefaultAcl.Count > 0)
        {
            accessObject.OwnAclSetId = await GetOrCreateSetAsync(ToMasks(type, type.Definition.DefaultAcl),
                cancellationToken);
        }

        accessObject.AclSetId = await ComputeEffectiveAsync(accessObject, cancellationToken);
        context.AccessObjects.Add(accessObject);
        await context.SaveChangesAsync(cancellationToken);
        return accessObject.AclSetId;
    }

    /// <summary>
    /// Forgets a deleted object. The descendants keep their rights until they are deleted or moved.
    /// </summary>
    public async Task RemoveAsync(ObjectRef obj, CancellationToken cancellationToken = default)
    {
        var type = registry.Get(obj.Type);
        var accessObject = await FindAsync(type, obj.Id, cancellationToken);
        if (accessObject is null)
            return;
        context.AccessObjects.Remove(accessObject);
        await context.SaveChangesAsync(cancellationToken);
    }

    internal async Task SyncAsync(ObjectRef obj, ObjectRef? parent, bool hasOwner, string? owner,
        CancellationToken cancellationToken)
    {
        var type = registry.Get(obj.Type);
        var accessObject = await FindAsync(type, obj.Id, cancellationToken);
        if (accessObject is null)
            return;

        if (hasOwner && accessObject.OwnerSubject != owner)
        {
            accessObject.OwnerSubject = owner;
            await context.SaveChangesAsync(cancellationToken);
        }

        var currentParentType = parent is null ? (int?)null : registry.Get(parent.Value.Type).ObjectTypeId;
        if (accessObject.ParentTypeId == currentParentType && accessObject.ParentId == parent?.Id)
            return;

        await MoveCoreAsync(type, accessObject, parent, cancellationToken);
    }

    private async Task MoveCoreAsync(ResolvedObjectType type, AccessObjectEntity accessObject, ObjectRef? newParent,
        CancellationToken cancellationToken)
    {
        if (newParent is { } parentRef)
        {
            var parentType = ValidateParent(type, parentRef);
            var ancestor = await FindAsync(parentType, parentRef.Id, cancellationToken);
            while (ancestor is not null)
            {
                if (ancestor.ObjectTypeId == accessObject.ObjectTypeId && ancestor.ObjectId == accessObject.ObjectId)
                    throw new ApiException("Invalid parent", "An object cannot be moved under its own descendant.",
                        StatusCodes.Status400BadRequest);
                ancestor = ancestor.ParentTypeId is { } typeId && ancestor.ParentId is { } id
                    ? await FindAsync(registry.Get(typeId), id, cancellationToken)
                    : null;
            }

            accessObject.ParentTypeId = parentType.ObjectTypeId;
            accessObject.ParentId = parentRef.Id;
        }
        else
        {
            accessObject.ParentTypeId = null;
            accessObject.ParentId = null;
        }

        await RecomputeAsync(accessObject, cancellationToken);
    }

    private ResolvedObjectType ValidateParent(ResolvedObjectType type, ObjectRef parent)
    {
        if (type.Definition.ParentType != parent.Type)
            throw new ApiException("Invalid parent",
                $"A '{type.Definition.Type}' object can only be placed under '{type.Definition.ParentType}'.",
                StatusCodes.Status400BadRequest);
        return registry.Get(parent.Type);
    }

    /// <summary>
    /// Recomputes the effective set of the object and of every descendant that inherits from it. A subtree whose
    /// effective set did not change is skipped.
    /// </summary>
    private async Task RecomputeAsync(AccessObjectEntity root, CancellationToken cancellationToken)
    {
        var changes = new Dictionary<int, Dictionary<long, long>>();
        var merged = new Dictionary<(long?, long?), long>();
        var queue = new Queue<AccessObjectEntity>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var effective = await ComputeEffectiveAsync(node, cancellationToken, merged);
            if (effective == node.AclSetId && !ReferenceEquals(node, root))
                continue;

            node.AclSetId = effective;
            if (!changes.TryGetValue(node.ObjectTypeId, out var byId))
                changes[node.ObjectTypeId] = byId = [];
            byId[node.ObjectId] = effective;

            var children = await context.AccessObjects
                .Where(a => a.ParentTypeId == node.ObjectTypeId && a.ParentId == node.ObjectId && !a.InheritanceBroken)
                .ToListAsync(cancellationToken);
            foreach (var child in children)
                queue.Enqueue(child);
        }

        foreach (var (typeId, byId) in changes)
            await registry.Get(typeId).Definition.UpdateAclSetIdsAsync(context, byId, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<long> ComputeEffectiveAsync(AccessObjectEntity node, CancellationToken cancellationToken,
        Dictionary<(long?, long?), long>? merged = null)
    {
        long? inherited = null;
        if (!node.InheritanceBroken && node.ParentTypeId is { } parentTypeId && node.ParentId is { } parentId)
            inherited = (await FindAsync(registry.Get(parentTypeId), parentId, cancellationToken))?.AclSetId;

        var key = (inherited, node.OwnAclSetId);
        if (merged is not null && merged.TryGetValue(key, out var known))
            return known;

        var masks = new Dictionary<string, Masks>(StringComparer.Ordinal);
        foreach (var setId in new[] { inherited, node.OwnAclSetId }.OfType<long>())
        foreach (var (subject, entry) in await LoadSetAsync(setId, cancellationToken))
            masks[subject] = masks.TryGetValue(subject, out var current) ? current | entry : entry;

        var result = await GetOrCreateSetAsync(masks, cancellationToken);
        if (merged is not null)
            merged[key] = result;
        return result;
    }

    private async Task<long> GetEffectiveMaskAsync(ResolvedObjectType type, long objectId, SubjectSet subjects,
        CancellationToken cancellationToken)
    {
        if (subjects.IsAdmin)
            return type.AllRightsMask;
        if (!subjects.IsAuthenticated)
            return 0;

        var accessObject = await FindAsync(type, objectId, cancellationToken);
        if (accessObject is null)
            return 0;

        var isOwner = subjects.UserSubject is not null && accessObject.OwnerSubject == subjects.UserSubject;
        var keys = subjects.Keys.ToList();
        var entries = await context.AclSetEntries.AsNoTracking()
            .Where(a => a.AclSetId == accessObject.AclSetId &&
                        (keys.Contains(a.SubjectKey) || (isOwner && a.SubjectKey == SubjectKeys.Owner)))
            .ToListAsync(cancellationToken);

        var allow = entries.Aggregate(0L, (mask, e) => mask | e.AllowMask);
        var deny = entries.Aggregate(0L, (mask, e) => mask | e.DenyMask);
        return allow & ~deny & type.AllRightsMask;
    }

    private async Task DemandManageAclAsync(ResolvedObjectType type, AccessObjectEntity accessObject,
        CancellationToken cancellationToken)
    {
        var mask = await GetEffectiveMaskAsync(type, accessObject.ObjectId, subjectResolver.Resolve(), cancellationToken);
        if ((mask & type.MaskOf(SecurityConstants.ManageAcl)) == 0)
            throw new ApiException("Forbidden",
                $"The '{SecurityConstants.ManageAcl}' right on {type.Definition.Type}:{accessObject.ObjectId} is required.",
                StatusCodes.Status403Forbidden);
    }

    private Task<AccessObjectEntity?> FindAsync(ResolvedObjectType type, long objectId,
        CancellationToken cancellationToken)
    {
        var local = context.AccessObjects.Local
            .FirstOrDefault(a => a.ObjectTypeId == type.ObjectTypeId && a.ObjectId == objectId);
        return local is not null
            ? Task.FromResult<AccessObjectEntity?>(local)
            : context.AccessObjects.FirstOrDefaultAsync(
                a => a.ObjectTypeId == type.ObjectTypeId && a.ObjectId == objectId, cancellationToken);
    }

    private async Task<Dictionary<string, Masks>> LoadSetAsync(long aclSetId, CancellationToken cancellationToken)
    {
        if (_setCache.TryGetValue(aclSetId, out var cached))
            return cached;

        var entries = await context.AclSetEntries.AsNoTracking()
            .Where(a => a.AclSetId == aclSetId)
            .ToListAsync(cancellationToken);
        var result = entries.ToDictionary(e => e.SubjectKey, e => new Masks(e.AllowMask, e.DenyMask),
            StringComparer.Ordinal);
        _setCache[aclSetId] = result;
        return result;
    }

    private async Task<long> GetOrCreateSetAsync(IReadOnlyDictionary<string, Masks> masks,
        CancellationToken cancellationToken)
    {
        var normalized = masks.Where(m => m.Value.Allow != 0 || m.Value.Deny != 0)
            .OrderBy(m => m.Key, StringComparer.Ordinal)
            .ToList();
        var canonical = string.Join('\n', normalized.Select(m => $"{m.Key}\u001f{m.Value.Allow}\u001f{m.Value.Deny}"));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));

        var existing = await context.AclSets.AsNoTracking()
            .Where(s => s.Hash == hash)
            .Select(s => (long?)s.AclSetId)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
            return existing.Value;

        var set = new AclSetEntity
        {
            Hash = hash,
            Entries = normalized.Select(m => new AclSetEntryEntity
            {
                SubjectKey = m.Key,
                AllowMask = m.Value.Allow,
                DenyMask = m.Value.Deny
            }).ToList()
        };
        context.AclSets.Add(set);
        await context.SaveChangesAsync(cancellationToken);
        _setCache[set.AclSetId] = normalized.ToDictionary(m => m.Key, m => m.Value, StringComparer.Ordinal);
        return set.AclSetId;
    }

    private static Dictionary<string, Masks> ToMasks(ResolvedObjectType type, IEnumerable<AclEntry> entries)
    {
        var result = new Dictionary<string, Masks>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (!SubjectKeys.IsValid(entry.SubjectKey))
                throw new ApiException("Invalid subject", $"Subject key '{entry.SubjectKey}' is not valid.",
                    StatusCodes.Status400BadRequest);

            Masks masks;
            try
            {
                masks = new Masks(type.MaskOf(entry.Allow), type.MaskOf(entry.Deny ?? []));
            }
            catch (ArgumentException ex)
            {
                throw new ApiException("Invalid right", ex.Message, StatusCodes.Status400BadRequest);
            }

            if (masks.Allow == 0 && masks.Deny == 0)
                continue;
            result[entry.SubjectKey] = result.TryGetValue(entry.SubjectKey, out var current) ? current | masks : masks;
        }

        return result;
    }

    private static List<AclEntryDto> ToEntries(ResolvedObjectType type, Dictionary<string, Masks> set) =>
        set.OrderBy(e => e.Key, StringComparer.Ordinal)
            .Select(e => new AclEntryDto
            {
                SubjectKey = e.Key,
                SubjectType = SubjectKeys.GetType(e.Key),
                DisplayName = SubjectKeys.GetValue(e.Key),
                Allow = type.RightsOf(e.Value.Allow),
                Deny = type.RightsOf(e.Value.Deny)
            })
            .Where(e => e.Allow.Count > 0 || e.Deny.Count > 0)
            .ToList();

    private static AccessObjectTypeDto ToDto(SecurableObjectDefinition definition) => new()
    {
        Code = definition.Type,
        Rights = definition.Rights.Select(r => new AccessRightDto { Code = r.Code, Name = r.Name }).ToList()
    };

    private async Task InTransactionAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        if (!context.Database.IsRelational() || context.Database.CurrentTransaction is not null)
        {
            await action();
            return;
        }

        await using IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);
        await action();
        await transaction.CommitAsync(cancellationToken);
    }

    private static ApiException NotRegistered(ObjectRef obj) =>
        new("Object not found", $"Object {obj} is not known to the access control.", StatusCodes.Status404NotFound);

    private readonly record struct Masks(long Allow, long Deny)
    {
        public static Masks operator |(Masks left, Masks right) =>
            new(left.Allow | right.Allow, left.Deny | right.Deny);
    }
}
