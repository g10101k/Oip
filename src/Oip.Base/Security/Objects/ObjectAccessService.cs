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
    private readonly Dictionary<long, Dictionary<string, Grants>> _setCache = [];

    /// <summary>
    /// Registry of the context.
    /// </summary>
    public AccessControlRegistry<TContext> Registry => registry;

    /// <inheritdoc />
    public IReadOnlyList<AccessObjectTypeDto> GetObjectTypes() =>
        registry.Definitions.Select(d => ToDto(registry.Get(d.Type))).ToList();

    /// <inheritdoc />
    public bool IsRegistered(string objectType) => registry.Find(objectType) is not null;

    /// <inheritdoc />
    public Task<bool> HasRightAsync(ObjectRef obj, string right, CancellationToken cancellationToken = default) =>
        HasRightAsync(obj, right, subjectResolver.Resolve(), cancellationToken);

    /// <summary>
    /// Whether the given subjects hold the right on the object, for a check on behalf of someone else than the caller.
    /// </summary>
    public async Task<bool> HasRightAsync(ObjectRef obj, string right, SubjectSet subjects,
        CancellationToken cancellationToken = default)
    {
        if (subjects.IsAdmin)
            return true;
        var (type, resolved) = ResolveRight(obj.Type, right);
        if (resolved.IsNone)
            return false;
        var (mask, customIds) = await GetEffectiveAsync(type, obj.Id, subjects, cancellationToken);
        return resolved.Mask != 0 ? (mask & resolved.Mask) != 0 : customIds.Contains(resolved.CustomId);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetEffectiveRightsAsync(ObjectRef obj,
        CancellationToken cancellationToken = default) =>
        GetEffectiveRightsAsync(obj, subjectResolver.Resolve(), cancellationToken);

    /// <summary>
    /// Rights of the given subjects on the object.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetEffectiveRightsAsync(ObjectRef obj, SubjectSet subjects,
        CancellationToken cancellationToken = default)
    {
        var type = registry.Get(obj.Type);
        var (mask, customIds) = await GetEffectiveAsync(type, obj.Id, subjects, cancellationToken);
        return type.RightsOf(mask, customIds);
    }

    /// <summary>
    /// Keeps only the entities the caller holds the right on.
    /// </summary>
    public IQueryable<T> WhereAccessible<T>(IQueryable<T> query, string right) where T : class, ISecurableEntity =>
        WhereAccessible(query, right, subjectResolver.Resolve());

    /// <summary>
    /// Keeps only the entities the given subjects hold the right on.
    /// </summary>
    public IQueryable<T> WhereAccessible<T>(IQueryable<T> query, string right, SubjectSet subjects)
        where T : class, ISecurableEntity
    {
        var definition = registry.FindByEntity(typeof(T))
                         ?? throw new ArgumentException(
                             $"{typeof(T).Name} is not registered as a securable object in {typeof(TContext).Name}.");

        if (subjects.IsAdmin)
            return query;

        var (_, resolved) = ResolveRight(definition.Type, right);
        if (!subjects.IsAuthenticated || resolved.IsNone)
            return query.Where(_ => false);

        if (definition is SecurableObjectDefinition<T> { OwnerExpression: { } ownerOf } &&
            subjects.UserSubject is not null)
            return FilterOwned(query, resolved, subjects, ownerOf);

        return Filter(query, resolved, subjects);
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
            var granted = await WhereAccessible(context.Set<T>().AsNoTracking(), right, subjectResolver.Resolve())
                .Where(definition.IdIn(result.Keys))
                .Select(definition.IdExpression)
                .ToListAsync(cancellationToken);
            foreach (var id in granted)
                result[id].Add(right);
        }

        return result;
    }

    private IQueryable<T> Filter<T>(IQueryable<T> query, RightRef right, SubjectSet subjects)
        where T : class, ISecurableEntity
    {
        var keys = subjects.Keys.ToList();
        if (right.Mask != 0)
        {
            var mask = right.Mask;
            var entries = context.AclSetEntries;
            return query.Where(e =>
                entries.Any(a => a.AclSetId == e.AclSetId && keys.Contains(a.SubjectKey) && (a.AllowMask & mask) != 0) &&
                !entries.Any(a => a.AclSetId == e.AclSetId && keys.Contains(a.SubjectKey) && (a.DenyMask & mask) != 0));
        }

        var rightId = right.CustomId;
        var rows = context.AclSetEntryRights;
        return query.Where(e =>
            rows.Any(r => r.AclSetId == e.AclSetId && keys.Contains(r.SubjectKey) && r.RightId == rightId && !r.IsDeny) &&
            !rows.Any(r => r.AclSetId == e.AclSetId && keys.Contains(r.SubjectKey) && r.RightId == rightId && r.IsDeny));
    }

    private IQueryable<T> FilterOwned<T>(IQueryable<T> query, RightRef right, SubjectSet subjects,
        Expression<Func<T, string?>> ownerOf)
        where T : class, ISecurableEntity
    {
        var keys = subjects.Keys.ToList();
        var subject = subjects.UserSubject;
        const string owner = SubjectKeys.Owner;

        // The owner value is a placeholder that is replaced with the owner expression of the type below.
        Expression<Func<T, string?, bool>> template;
        if (right.Mask != 0)
        {
            var mask = right.Mask;
            var entries = context.AclSetEntries;
            template = (e, ownerValue) =>
                entries.Any(a => a.AclSetId == e.AclSetId &&
                                 (keys.Contains(a.SubjectKey) || (a.SubjectKey == owner && ownerValue == subject)) &&
                                 (a.AllowMask & mask) != 0) &&
                !entries.Any(a => a.AclSetId == e.AclSetId &&
                                  (keys.Contains(a.SubjectKey) || (a.SubjectKey == owner && ownerValue == subject)) &&
                                  (a.DenyMask & mask) != 0);
        }
        else
        {
            var rightId = right.CustomId;
            var rows = context.AclSetEntryRights;
            template = (e, ownerValue) =>
                rows.Any(r => r.AclSetId == e.AclSetId &&
                              (keys.Contains(r.SubjectKey) || (r.SubjectKey == owner && ownerValue == subject)) &&
                              r.RightId == rightId && !r.IsDeny) &&
                !rows.Any(r => r.AclSetId == e.AclSetId &&
                               (keys.Contains(r.SubjectKey) || (r.SubjectKey == owner && ownerValue == subject)) &&
                               r.RightId == rightId && r.IsDeny);
        }

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
    public Task<ObjectAclDto> GetAclAsync(ObjectRef obj, CancellationToken cancellationToken = default) =>
        GetAclCoreAsync(obj, true, cancellationToken);

    /// <summary>
    /// Explicit and inherited rights of the object without checking the rights of the caller. For seeding,
    /// migrations and background jobs only.
    /// </summary>
    public Task<ObjectAclDto> GetAclUncheckedAsync(ObjectRef obj, CancellationToken cancellationToken = default) =>
        GetAclCoreAsync(obj, false, cancellationToken);

    private async Task<ObjectAclDto> GetAclCoreAsync(ObjectRef obj, bool demandManageAcl,
        CancellationToken cancellationToken)
    {
        var type = registry.Get(obj.Type);
        var accessObject = await FindAsync(type, obj.Id, cancellationToken) ?? throw NotRegistered(obj);
        if (demandManageAcl)
            await DemandManageAclAsync(type, accessObject, cancellationToken);

        var result = new ObjectAclDto
        {
            ObjectType = obj.Type,
            ObjectId = obj.Id,
            InheritanceBroken = accessObject.InheritanceBroken,
            Rights = ToDto(type).Rights,
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
        var unknown = entries.SelectMany(e => e.Allow.Concat(e.Deny ?? [])).FirstOrDefault(r => !type.Knows(r));
        if (unknown is not null)
            type = registry.Get(obj.Type, unknown);
        var grants = ToGrants(type, entries);

        await InTransactionAsync(async () =>
        {
            accessObject.OwnAclSetId = grants.Count == 0 ? null : await GetOrCreateSetAsync(grants, cancellationToken);
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
            accessObject.OwnAclSetId = await GetOrCreateSetAsync(ToGrants(type, type.Definition.DefaultAcl),
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

    /// <summary>
    /// Resolves a right. A right that a type with stored rights does not know yet is granted to nobody but the admin,
    /// so a module may check a right before anyone assigned it.
    /// </summary>
    private (ResolvedObjectType Type, RightRef Right) ResolveRight(string objectType, string right)
    {
        var type = registry.Get(objectType, right);
        var mask = StandardRights.MaskOf(right);
        if (mask != 0)
            return (type, new RightRef(mask, 0));
        if (type.CustomRightIds.TryGetValue(right, out var id))
            return (type, new RightRef(0, id));
        if (type.Definition.AcceptsStoredRights)
            return (type, default);
        throw new ArgumentException($"Right '{right}' is not defined for '{objectType}'.", nameof(right));
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

        var grants = new Dictionary<string, Grants>(StringComparer.Ordinal);
        foreach (var setId in new[] { inherited, node.OwnAclSetId }.OfType<long>())
        foreach (var (subject, entry) in await LoadSetAsync(setId, cancellationToken))
            grants[subject] = grants.TryGetValue(subject, out var current) ? current.Union(entry) : entry;

        var result = await GetOrCreateSetAsync(grants, cancellationToken);
        if (merged is not null)
            merged[key] = result;
        return result;
    }

    /// <summary>
    /// Effective standard mask and custom right identifiers of the subjects on the object.
    /// </summary>
    private async Task<(int Mask, IReadOnlySet<int> CustomIds)> GetEffectiveAsync(ResolvedObjectType type,
        long objectId, SubjectSet subjects, CancellationToken cancellationToken)
    {
        if (subjects.IsAdmin)
            return (StandardRights.All, type.CustomRightIds.Values.ToHashSet());
        if (!subjects.IsAuthenticated)
            return (0, EmptyIds);

        var accessObject = await FindAsync(type, objectId, cancellationToken);
        if (accessObject is null)
            return (0, EmptyIds);

        var isOwner = subjects.UserSubject is not null && accessObject.OwnerSubject == subjects.UserSubject;
        var keys = subjects.Keys.ToHashSet(StringComparer.Ordinal);
        var granted = Grants.Empty;
        foreach (var (subject, entry) in await LoadSetAsync(accessObject.AclSetId, cancellationToken))
            if (keys.Contains(subject) || (isOwner && subject == SubjectKeys.Owner))
                granted = granted.Union(entry);

        var customIds = granted.CustomAllow.Except(granted.CustomDeny)
            .Where(id => type.CustomRightIds.Values.Contains(id))
            .ToHashSet();
        return (granted.Allow & ~granted.Deny & StandardRights.All, customIds);
    }

    private async Task DemandManageAclAsync(ResolvedObjectType type, AccessObjectEntity accessObject,
        CancellationToken cancellationToken)
    {
        var (mask, _) = await GetEffectiveAsync(type, accessObject.ObjectId, subjectResolver.Resolve(),
            cancellationToken);
        if ((mask & StandardRights.MaskOf(SecurityConstants.ManageAcl)) == 0)
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

    private async Task<Dictionary<string, Grants>> LoadSetAsync(long aclSetId, CancellationToken cancellationToken)
    {
        if (_setCache.TryGetValue(aclSetId, out var cached))
            return cached;

        var entries = await context.AclSetEntries.AsNoTracking()
            .Where(a => a.AclSetId == aclSetId)
            .ToListAsync(cancellationToken);
        var rows = (await context.AclSetEntryRights.AsNoTracking()
                .Where(r => r.AclSetId == aclSetId)
                .ToListAsync(cancellationToken))
            .ToLookup(r => r.SubjectKey, StringComparer.Ordinal);

        var result = entries.ToDictionary(e => e.SubjectKey, e => new Grants(e.AllowMask, e.DenyMask,
                new SortedSet<int>(rows[e.SubjectKey].Where(r => !r.IsDeny).Select(r => r.RightId)),
                new SortedSet<int>(rows[e.SubjectKey].Where(r => r.IsDeny).Select(r => r.RightId))),
            StringComparer.Ordinal);
        _setCache[aclSetId] = result;
        return result;
    }

    private async Task<long> GetOrCreateSetAsync(IReadOnlyDictionary<string, Grants> grants,
        CancellationToken cancellationToken)
    {
        var normalized = grants.Where(g => !g.Value.IsEmpty)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
        var canonical = string.Join('\n', normalized.Select(g =>
            $"{g.Key}\u001f{g.Value.Allow}\u001f{g.Value.Deny}\u001f{string.Join(',', g.Value.CustomAllow)}" +
            $"\u001f{string.Join(',', g.Value.CustomDeny)}"));
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
            Entries = normalized.Select(g => new AclSetEntryEntity
            {
                SubjectKey = g.Key,
                AllowMask = g.Value.Allow,
                DenyMask = g.Value.Deny,
                Rights = g.Value.CustomAllow.Select(id => new AclSetEntryRightEntity
                        { SubjectKey = g.Key, RightId = id, IsDeny = false })
                    .Concat(g.Value.CustomDeny.Select(id => new AclSetEntryRightEntity
                        { SubjectKey = g.Key, RightId = id, IsDeny = true }))
                    .ToList()
            }).ToList()
        };
        context.AclSets.Add(set);
        await context.SaveChangesAsync(cancellationToken);
        _setCache[set.AclSetId] = normalized.ToDictionary(g => g.Key, g => g.Value, StringComparer.Ordinal);
        return set.AclSetId;
    }

    private static Dictionary<string, Grants> ToGrants(ResolvedObjectType type, IEnumerable<AclEntry> entries)
    {
        var result = new Dictionary<string, Grants>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (!SubjectKeys.IsValid(entry.SubjectKey))
                throw new ApiException("Invalid subject", $"Subject key '{entry.SubjectKey}' is not valid.",
                    StatusCodes.Status400BadRequest);

            Grants grants;
            try
            {
                var allow = type.Encode(entry.Allow);
                var deny = type.Encode(entry.Deny ?? []);
                grants = new Grants(allow.Mask, deny.Mask, new SortedSet<int>(allow.CustomIds),
                    new SortedSet<int>(deny.CustomIds));
            }
            catch (ArgumentException ex)
            {
                throw new ApiException("Invalid right", ex.Message, StatusCodes.Status400BadRequest);
            }

            if (grants.IsEmpty)
                continue;
            result[entry.SubjectKey] = result.TryGetValue(entry.SubjectKey, out var current)
                ? current.Union(grants)
                : grants;
        }

        return result;
    }

    private static List<AclEntryDto> ToEntries(ResolvedObjectType type, Dictionary<string, Grants> set) =>
        set.OrderBy(e => e.Key, StringComparer.Ordinal)
            .Select(e => new AclEntryDto
            {
                SubjectKey = e.Key,
                SubjectType = SubjectKeys.GetType(e.Key),
                DisplayName = SubjectKeys.GetValue(e.Key),
                Allow = type.RightsOf(e.Value.Allow, e.Value.CustomAllow),
                Deny = type.RightsOf(e.Value.Deny, e.Value.CustomDeny)
            })
            .Where(e => e.Allow.Count > 0 || e.Deny.Count > 0)
            .ToList();

    private static AccessObjectTypeDto ToDto(ResolvedObjectType type) => new()
    {
        Code = type.Definition.Type,
        Rights = type.RightCodes.Select(code => new AccessRightDto
        {
            Code = code,
            Name = type.Definition.Rights.FirstOrDefault(r => r.Code == code)?.Name
        }).ToList()
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

    private static readonly IReadOnlySet<int> EmptyIds = new HashSet<int>();

    /// <summary>
    /// Right of a check: a standard right mask, or a custom right identifier. Neither for a stored right not
    /// assigned yet.
    /// </summary>
    private readonly record struct RightRef(int Mask, int CustomId)
    {
        public bool IsNone => Mask == 0 && CustomId == 0;
    }

    /// <summary>
    /// Rights of one subject in a set: masks of the standard rights and identifiers of the custom rights.
    /// </summary>
    private sealed record Grants(int Allow, int Deny, SortedSet<int> CustomAllow, SortedSet<int> CustomDeny)
    {
        public static readonly Grants Empty = new(0, 0, [], []);

        public bool IsEmpty => Allow == 0 && Deny == 0 && CustomAllow.Count == 0 && CustomDeny.Count == 0;

        public Grants Union(Grants other) => new(Allow | other.Allow, Deny | other.Deny,
            [..CustomAllow, ..other.CustomAllow], [..CustomDeny, ..other.CustomDeny]);
    }
}
