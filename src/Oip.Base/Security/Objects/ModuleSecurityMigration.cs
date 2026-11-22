using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oip.Base.Data.Contexts;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Moves the module instance rights from the role and right pairs of <c>ModuleInstanceSecurity</c> to the object
/// access control. Each instance keeps exactly the access it had: its rights become explicit and it does not inherit
/// from its folder. Instances already known to the access control are skipped, so the move runs once per instance.
/// </summary>
public static class ModuleSecurityMigration
{
    /// <summary>
    /// Registers the rights the modules declare and moves the rights of the instances not migrated yet.
    /// </summary>
    /// <param name="context">Module context.</param>
    /// <param name="access">Access control of the module context.</param>
    /// <param name="declaredRights">Rights the loaded modules declare in <c>GetModuleRights</c>.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of the migrated instances.</returns>
    public static async Task<int> MigrateAsync(OipModuleContext context, ObjectAccessService<OipModuleContext> access,
        IEnumerable<string> declaredRights, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var legacy = await context.ModuleInstanceSecurities.AsNoTracking()
            .Select(x => new { x.ModuleInstanceId, x.Right, x.Role })
            .ToListAsync(cancellationToken);
        access.Registry.EnsureRights(ModuleAccessControl.ModuleInstanceType,
            declaredRights.Concat(legacy.Select(x => x.Right)).Distinct());

        var typeId = access.Registry.Get(ModuleAccessControl.ModuleInstanceType).ObjectTypeId;
        var known = (await context.AccessObjects.AsNoTracking()
                .Where(o => o.ObjectTypeId == typeId)
                .Select(o => o.ObjectId)
                .ToListAsync(cancellationToken))
            .ToHashSet();
        var instances = await context.ModuleInstances.AsNoTracking()
            .Select(x => new { x.ModuleInstanceId, x.ParentId })
            .ToListAsync(cancellationToken);
        var pending = instances.Where(x => !known.Contains(x.ModuleInstanceId)).ToList();
        if (pending.Count == 0)
            return 0;

        var existing = instances.Select(x => x.ModuleInstanceId).ToHashSet();
        var rightsByInstance = legacy.ToLookup(x => x.ModuleInstanceId);
        var registered = new HashSet<long>(known);
        var migrated = 0;

        // Parents go first, so a child is registered under a folder that already exists in the access control.
        while (pending.Count > 0)
        {
            var ready = pending.Where(x => x.ParentId is not { } parentId || registered.Contains(parentId) ||
                                           !existing.Contains(parentId)).ToList();
            if (ready.Count == 0)
            {
                logger?.LogWarning("Module instances {Ids} form a parent cycle and keep no rights.",
                    string.Join(", ", pending.Select(x => x.ModuleInstanceId)));
                break;
            }

            foreach (var instance in ready)
            {
                var obj = ModuleAccessControl.Instance(instance.ModuleInstanceId);
                var parent = instance.ParentId is { } parentId && existing.Contains(parentId)
                    ? ModuleAccessControl.Instance(parentId)
                    : (ObjectRef?)null;
                var entries = rightsByInstance[instance.ModuleInstanceId]
                    .GroupBy(x => x.Role)
                    .Select(g => new AclEntry(SubjectKeys.Role(g.Key), g.Select(x => x.Right).Distinct().ToList()))
                    .ToList();

                await access.RegisterAsync(obj, parent, null, cancellationToken);
                await access.SetAclUncheckedAsync(obj, true, entries, cancellationToken);
                registered.Add(instance.ModuleInstanceId);
                pending.Remove(instance);
                migrated++;
            }
        }

        logger?.LogInformation("Moved the rights of {Count} module instances to the object access control.", migrated);
        return migrated;
    }
}
