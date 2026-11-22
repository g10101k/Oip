using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Extensions;
using Oip.Base.Exceptions;
using Oip.Base.Security.Objects;
using Oip.Base.Security.Objects.Dtos;

namespace Oip.Base.Data.Repositories;

/// <summary>
/// Provides methods to manage modules and their instances, including CRUD operations and security settings.
/// </summary>
/// <remarks>
/// This repository encapsulates access to module data, such as module definitions, settings, instances, and associated security rules.
/// </remarks>
public class ModuleRepository(OipModuleContext db, ObjectAccessService<OipModuleContext>? access = null)
{
    private ObjectAccessService<OipModuleContext> Access => access ?? throw new InvalidOperationException(
        "Access control of the module instances is not registered. Register the context with AddOipModuleContext.");

    /// <summary>
    /// Options used to read module instance settings. Property names are matched case-insensitively so that
    /// settings persisted before the switch to <see cref="JsonSerializer"/> keep deserializing.
    /// </summary>
    private static readonly JsonSerializerOptions SettingsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Retrieves all available modules with their associated security settings.
    /// </summary>
    /// <return>A collection of module DTOs.</return>
    public async Task<IEnumerable<ModuleDto>> GetAll()
    {
        var query = from module in db.Modules
            join moduleSecurity in db.ModuleSecurities on module.ModuleId equals moduleSecurity.ModuleId into
                security
            select new ModuleDto
            {
                ModuleId = module.ModuleId,
                Settings = module.Settings,
                Name = module.Name,
                Kind = module.Kind,
                ManifestUrl = module.ManifestUrl,
                ExtensionKey = module.ExtensionKey,
                LoadType = module.LoadType,
                ElementName = module.ElementName,
                ScriptUrl = module.ScriptUrl,
                RemoteEntryUrl = module.RemoteEntryUrl,
                ExposedModule = module.ExposedModule,
                ComponentName = module.ComponentName,
                ApiBaseUrl = module.ApiBaseUrl,
                Version = module.Version,
                ModuleSecurities = security.Select(x => new ModuleSecurityDto
                {
                    Right = x.Right,
                    Role = x.Role
                })
            };
        var result = await query.ToListAsync();
        return result;
    }

    /// <summary>
    /// Inserts a list of new modules with associated security settings into the database.
    /// </summary>
    /// <param name="list">The list of modules to insert.</param>
    public async Task Insert(IEnumerable<ModuleDto> list)
    {
        await db.Modules.AddRangeAsync(list.Select(x =>
            new ModuleEntity
            {
                Name = x.Name,
                Settings = x.Settings,
                Kind = x.Kind,
                ManifestUrl = x.ManifestUrl,
                ExtensionKey = x.ExtensionKey,
                LoadType = x.LoadType,
                ElementName = x.ElementName,
                ScriptUrl = x.ScriptUrl,
                RemoteEntryUrl = x.RemoteEntryUrl,
                ExposedModule = x.ExposedModule,
                ComponentName = x.ComponentName,
                ApiBaseUrl = x.ApiBaseUrl,
                Version = x.Version,
                ModuleSecurities = x.ModuleSecurities.Select(xx => new ModuleSecurityEntity()
                {
                    ModuleId = x.ModuleId,
                    Right = xx.Right,
                    Role = xx.Role
                }).ToList()
            }
        ));
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all module instances available in the menu, filtered by user roles.
    /// </summary>
    /// <param name="roles">List of user roles to filter modules by access.</param>
    /// <param name="userSubject">Subject of the current user, used to mark the start module instance.</param>
    /// <returns>A collection of module instances accessible to the specified roles.</returns>
    public async Task<IEnumerable<ModuleInstanceDto>> GetModuleForMenuAll(List<string> roles,
        string? userSubject = null)
    {
        var loadedModules = await WebApplicationBuilderExtension.GetAllLoadedModulesAsync();
        var modulesLoadedNames = loadedModules.Select(x => x.Name.Replace("Controller", string.Empty)).ToList();
        var subjects = ModuleAccessControl.SubjectsOf(roles, userSubject);
        var query = await Access.WhereAccessible(db.ModuleInstances, SecurityConstants.Read, subjects)
            .Include(x => x.Module)
            .Where(x => x.Module.Kind == ModuleKind.Extension ||
                        modulesLoadedNames.Contains(x.Module.Name))
            .OrderBy(m => m.Order)
            .ToListAsync();

        var startModuleInstanceId = await GetStartModuleInstanceId(userSubject);
        var viewRoles = await GetViewRolesAsync(query.Select(x => (long)x.ModuleInstanceId).ToList());
        // The tree is built from the filtered list, not from the Items navigation, which may also hold children
        // the context tracks for another reason.
        var children = query.ToLookup(x => x.ParentId);
        var result = query.Where(x => x.ParentId == null)
            .Select(x => ToDto(x, startModuleInstanceId, viewRoles, children));

        return result.ToList();
    }

    /// <summary>
    /// Retrieves the roles and the rights they are granted explicitly on a module instance.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <returns>A role and right pair for every right granted to a role.</returns>
    public async Task<List<ModuleSecurityDto>> GetSecurityByInstanceId(int id)
    {
        var acl = await FindAclAsync(id);
        return acl?.Entries
            .Where(e => e.SubjectType == SubjectKeys.RoleType)
            .SelectMany(e => e.Allow.Select(right => new ModuleSecurityDto
            {
                Right = right,
                Role = SubjectKeys.GetValue(e.SubjectKey)
            }))
            .ToList() ?? [];
    }

    /// <summary>
    /// Determines whether any of the specified roles grants the given right on a module instance.
    /// </summary>
    /// <param name="moduleInstanceId">The ID of the module instance.</param>
    /// <param name="roles">The roles of the current user.</param>
    /// <param name="right">The right to check, see <see cref="SecurityConstants"/>.</param>
    /// <returns><c>true</c> when the right is granted; otherwise <c>false</c>.</returns>
    public Task<bool> HasInstanceRight(int moduleInstanceId, List<string> roles, string right) =>
        Access.HasRightAsync(ModuleAccessControl.Instance(moduleInstanceId), right,
            ModuleAccessControl.SubjectsOf(roles));

    /// <summary>
    /// Retrieves the rights the specified roles hold on a module instance, inherited ones included.
    /// </summary>
    /// <param name="moduleInstanceId">The ID of the module instance.</param>
    /// <param name="roles">The roles of the current user.</param>
    /// <returns>A distinct list of rights. Administrators get every right of the module instances.</returns>
    public async Task<List<string>> GetUserRightsByInstanceId(int moduleInstanceId, List<string> roles) =>
        (await Access.GetEffectiveRightsAsync(ModuleAccessControl.Instance(moduleInstanceId),
            ModuleAccessControl.SubjectsOf(roles))).ToList();

    /// <summary>
    /// Replaces the rights granted to roles on a module instance. Entries of other subjects, the denied rights of
    /// the roles and the inheritance setting are kept.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <param name="security">The rights granted to each role.</param>
    public async Task UpdateInstanceSecurity(int id, IEnumerable<ModuleSecurityDto> security)
    {
        var pairs = security.ToList();
        Access.Registry.EnsureRights(ModuleAccessControl.ModuleInstanceType, pairs.Select(x => x.Right));

        var obj = ModuleAccessControl.Instance(id);
        var acl = await Access.GetAclUncheckedAsync(obj);
        var denied = acl.Entries.Where(e => e.SubjectType == SubjectKeys.RoleType)
            .ToDictionary(e => e.SubjectKey, e => e.Deny);

        var entries = acl.Entries.Where(e => e.SubjectType != SubjectKeys.RoleType).Select(ToAclEntry).ToList();
        var granted = pairs.GroupBy(x => SubjectKeys.Role(x.Role))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Right).Distinct().ToList());
        entries.AddRange(granted.Keys.Union(denied.Keys).Select(key => new AclEntry(key,
            granted.GetValueOrDefault(key) ?? [], denied.GetValueOrDefault(key) ?? [])));

        await Access.SetAclUncheckedAsync(obj, acl.InheritanceBroken, entries);
    }

    /// <summary>
    /// Grants the read right to exactly the given roles on a module instance, keeping every other entry.
    /// </summary>
    private async Task SetViewRolesAsync(int id, IReadOnlyCollection<string> viewRoles)
    {
        var obj = ModuleAccessControl.Instance(id);
        var acl = await Access.GetAclUncheckedAsync(obj);
        var keys = viewRoles.Select(SubjectKeys.Role).ToHashSet();

        var entries = acl.Entries.Select(e =>
        {
            if (e.SubjectType != SubjectKeys.RoleType)
                return ToAclEntry(e);
            var allow = e.Allow.Where(r => r != SecurityConstants.Read || keys.Contains(e.SubjectKey)).ToList();
            if (keys.Contains(e.SubjectKey) && !allow.Contains(SecurityConstants.Read))
                allow.Add(SecurityConstants.Read);
            return new AclEntry(e.SubjectKey, allow, e.Deny);
        }).ToList();
        entries.AddRange(keys.Where(key => acl.Entries.All(e => e.SubjectKey != key))
            .Select(key => new AclEntry(key, [SecurityConstants.Read])));

        await Access.SetAclUncheckedAsync(obj, acl.InheritanceBroken, entries);
    }

    /// <summary>
    /// Roles granted the read right explicitly, by module instance.
    /// </summary>
    private async Task<Dictionary<long, List<string>>> GetViewRolesAsync(List<long>? ids = null)
    {
        var type = Access.Registry.Get(ModuleAccessControl.ModuleInstanceType);
        var readMask = StandardRights.MaskOf(SecurityConstants.Read);
        const string rolePrefix = SubjectKeys.RoleType + ":";

        var objects = db.AccessObjects.Where(o => o.ObjectTypeId == type.ObjectTypeId && o.OwnAclSetId != null);
        if (ids is not null)
            objects = objects.Where(o => ids.Contains(o.ObjectId));

        var rows = await (from o in objects
                join e in db.AclSetEntries on o.OwnAclSetId equals e.AclSetId
                where e.SubjectKey.StartsWith(rolePrefix) && (e.AllowMask & readMask) != 0 &&
                      (e.DenyMask & readMask) == 0
                select new { o.ObjectId, e.SubjectKey })
            .AsNoTracking()
            .ToListAsync();

        return rows.GroupBy(x => x.ObjectId)
            .ToDictionary(g => g.Key, g => g.Select(x => SubjectKeys.GetValue(x.SubjectKey)).OrderBy(x => x).ToList());
    }

    private async Task<ObjectAclDto?> FindAclAsync(int id)
    {
        try
        {
            return await Access.GetAclUncheckedAsync(ModuleAccessControl.Instance(id));
        }
        catch (ApiException e) when (e.StatusCode == 404)
        {
            return null;
        }
    }

    private static AclEntry ToAclEntry(AclEntryDto entry) => new(entry.SubjectKey, entry.Allow, entry.Deny);

    /// <summary>
    /// Converts a <see cref="ModuleInstanceEntity"/> to a <see cref="ModuleInstanceDto"/>.
    /// </summary>
    /// <param name="module">The <see cref="ModuleInstanceEntity"/> to convert.</param>
    /// <param name="startModuleInstanceId">Module instance marked as the start page of the current user.</param>
    /// <return>The converted <see cref="ModuleInstanceDto"/>.</return>
    private static ModuleInstanceDto ToDto(ModuleInstanceEntity module, int? startModuleInstanceId = null,
        IReadOnlyDictionary<long, List<string>>? viewRoles = null, ILookup<int?, ModuleInstanceEntity>? children = null)
    {
        var items = children is null ? module.Items : children[module.ModuleInstanceId].ToList();
        return new ModuleInstanceDto
        {
            IsStart = startModuleInstanceId == module.ModuleInstanceId,
            ModuleInstanceId = module.ModuleInstanceId,
            ModuleId = module.ModuleId,
            ParentId = module.ParentId,
            Label = module.Label,
            Order = module.Order,
            Icon = module.Icon,
            RouterLink = module.Module.RouterLink != null
                ? [UrlAppend(module.Module.RouterLink, module.ModuleInstanceId)]
                : null,
            Url = module.Url,
            Target = module.Target,
            Settings = module.Settings,
            IsFolder = module.Module.Name == ModuleConstants.FolderModuleName,
            Items = items.Count == 0
                ? null
                : items.Select(x => ToDto(x, startModuleInstanceId, viewRoles, children)).ToList(),
            Securities = viewRoles?.GetValueOrDefault(module.ModuleInstanceId) ?? []
        };
    }

    /// <summary>
    /// Appends a URL part to the base URL.
    /// </summary>
    /// <param name="url">The base URL.</param>
    /// <param name="part">The URL part to append.</param>
    /// <returns>The resulting URL.</returns>
    private static string UrlAppend(string? url, int? part)
    {
        return string.IsNullOrEmpty(url) ? string.Empty : $"{url.TrimEnd('/')}/{part}";
    }

    /// <summary>
    /// Retrieves the settings string associated with a specific module instance.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <returns>The settings string, or <c>null</c> when the module instance has no settings.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the module instance is not found.</exception>
    public string? GetModuleInstanceSettings(int id)
    {
        var instance = db.ModuleInstances.Where(x => x.ModuleInstanceId == id).Select(x => new { x.Settings })
                           .FirstOrDefault()
                       ?? throw new KeyNotFoundException($"Module instance with id {id} not found");
        return instance.Settings;
    }

    /// <summary>
    /// Retrieves the settings of a specific module instance deserialized into <typeparamref name="TSettings"/>.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <typeparam name="TSettings">The type the settings are deserialized into.</typeparam>
    /// <returns>The deserialized settings, or a new instance when the module instance has no settings yet.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the module instance is not found.</exception>
    public TSettings GetModuleInstanceSettings<TSettings>(int id) where TSettings : class, new()
    {
        var settings = GetModuleInstanceSettings(id);
        if (string.IsNullOrWhiteSpace(settings))
            return new TSettings();
        return JsonSerializer.Deserialize<TSettings>(settings, SettingsJsonOptions) ?? new TSettings();
    }

    /// <summary>
    /// Updates the settings string of a specified module instance.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <param name="settings">The new settings string.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the module instance is not found.</exception>
    public void UpdateModuleInstanceSettings(int id, string settings)
    {
        var moduleInstance = db.ModuleInstances.FirstOrDefault(x => x.ModuleInstanceId == id);

        if (moduleInstance == null)
            throw new KeyNotFoundException($"Module instance with id {id} not found");

        moduleInstance.Settings = settings;

        db.SaveChanges();
    }

    /// <summary>
    /// Retrieves the full list of module instances accessible by administrators.
    /// </summary>
    /// <returns>A collection of all top-level module instances with admin access.</returns>
    public async Task<IEnumerable<ModuleInstanceDto>> GetAdminMenu()
    {
        var query = from module in db.ModuleInstances
                .Include(x => x.Module)
            select module;
        var viewRoles = await GetViewRolesAsync();
        var result = (await query.ToListAsync()).Where(x => x.Parent == null).Select(x => ToDto(x, null, viewRoles));

        return result.ToList();
    }

    /// <summary>
    /// Retrieves a list of all modules as key-value pairs.
    /// </summary>
    /// <returns>A list of modules with ID, name and default icon.</returns>
    public async Task<IEnumerable<ModuleKeyValueDto>> GetModules()
    {
        var query = from module in db.Modules
            orderby module.Name
            select new ModuleKeyValueDto(module.ModuleId, module.Name, module.Icon,
                module.Name == ModuleConstants.FolderModuleName);
        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Gets an extension module by key.
    /// </summary>
    /// <param name="extensionKey">Stable extension key.</param>
    /// <returns>The module DTO.</returns>
    public async Task<ModuleDto?> GetExtensionModuleByKey(string extensionKey)
    {
        var query = from module in db.Modules
            where module.Kind == ModuleKind.Extension && module.ExtensionKey == extensionKey
            select new ModuleDto
            {
                ModuleId = module.ModuleId,
                Settings = module.Settings,
                Name = module.Name,
                Kind = module.Kind,
                ManifestUrl = module.ManifestUrl,
                ExtensionKey = module.ExtensionKey,
                LoadType = module.LoadType,
                ElementName = module.ElementName,
                ScriptUrl = module.ScriptUrl,
                RemoteEntryUrl = module.RemoteEntryUrl,
                ExposedModule = module.ExposedModule,
                ComponentName = module.ComponentName,
                ApiBaseUrl = module.ApiBaseUrl,
                Version = module.Version,
                ModuleSecurities = module.ModuleSecurities.Select(x => new ModuleSecurityDto
                {
                    Right = x.Right,
                    Role = x.Role
                })
            };
        var result = await query.AsNoTracking().FirstOrDefaultAsync();
        return result;
    }

    /// <summary>
    /// Registers an extension module.
    /// </summary>
    public async Task<ModuleDto> RegisterExtensionModule(ExtensionModuleManifestDto manifest, string manifestUrl)
    {
        if (await db.Modules.AnyAsync(x => x.ExtensionKey == manifest.Key))
        {
            throw new InvalidOperationException($"Extension module with key '{manifest.Key}' already exists");
        }

        var module = new ModuleEntity
        {
            Name = manifest.Name,
            Settings = manifest.SettingsSchema?.ToJsonString(),
            RouterLink = $"/extensions/{manifest.Key}",
            Icon = manifest.Icon,
            Kind = ModuleKind.Extension,
            ManifestUrl = manifestUrl,
            ExtensionKey = manifest.Key,
            LoadType = manifest.LoadType,
            ElementName = manifest.ElementName,
            ScriptUrl = manifest.ScriptUrl,
            RemoteEntryUrl = manifest.RemoteEntryUrl,
            ExposedModule = manifest.ExposedModule,
            ComponentName = manifest.ComponentName,
            ApiBaseUrl = manifest.ApiBaseUrl,
            Version = manifest.Version,
            ModuleSecurities =
            [
                new ModuleSecurityEntity
                {
                    Right = "read",
                    Role = "admin"
                }
            ]
        };

        db.Modules.Add(module);
        await db.SaveChangesAsync();
        return ToModuleDto(module);
    }

    /// <summary>
    /// Updates an extension module.
    /// </summary>
    public async Task<ModuleDto> UpdateExtensionModule(int moduleId, ExtensionModuleManifestDto manifest,
        string manifestUrl)
    {
        var module = await db.Modules.FirstOrDefaultAsync(x => x.ModuleId == moduleId && x.Kind == ModuleKind.Extension)
                     ?? throw new KeyNotFoundException($"Extension module with id {moduleId} not found");

        var duplicate = await db.Modules.AnyAsync(x => x.ModuleId != moduleId && x.ExtensionKey == manifest.Key);
        if (duplicate)
        {
            throw new InvalidOperationException($"Extension module with key '{manifest.Key}' already exists");
        }

        module.Name = manifest.Name;
        module.Settings = manifest.SettingsSchema?.ToJsonString();
        module.RouterLink = $"/extensions/{manifest.Key}";
        module.Icon = manifest.Icon;
        module.ManifestUrl = manifestUrl;
        module.ExtensionKey = manifest.Key;
        module.LoadType = manifest.LoadType;
        module.ElementName = manifest.ElementName;
        module.ScriptUrl = manifest.ScriptUrl;
        module.RemoteEntryUrl = manifest.RemoteEntryUrl;
        module.ExposedModule = manifest.ExposedModule;
        module.ComponentName = manifest.ComponentName;
        module.ApiBaseUrl = manifest.ApiBaseUrl;
        module.Version = manifest.Version;

        await db.SaveChangesAsync();
        return ToModuleDto(module);
    }

    /// <summary>
    /// Adds a new module instance to the system and grants the <see cref="SecurityConstants.AdminRole"/> role
    /// every right declared by the module, plus <see cref="SecurityConstants.Read"/>.
    /// </summary>
    /// <param name="addModuleInstanceDto">The data transfer object containing module instance details.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the module or the parent module instance does not exist.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a non-folder module is placed at the menu root or the parent module instance is not a folder.
    /// </exception>
    public async Task AddModuleInstance(AddModuleInstanceDto addModuleInstanceDto)
    {
        var moduleName = await db.Modules.Where(x => x.ModuleId == addModuleInstanceDto.ModuleId)
                             .Select(x => x.Name)
                             .FirstOrDefaultAsync()
                         ?? throw new KeyNotFoundException(
                             $"Module with id {addModuleInstanceDto.ModuleId} not found");

        if (addModuleInstanceDto.ParentId == null)
        {
            if (moduleName != ModuleConstants.FolderModuleName)
                throw new InvalidOperationException("Only a folder can be placed at the menu root");
        }
        else
        {
            var parentModuleName = await db.ModuleInstances
                                       .Where(x => x.ModuleInstanceId == addModuleInstanceDto.ParentId)
                                       .Select(x => x.Module.Name)
                                       .FirstOrDefaultAsync()
                                   ?? throw new KeyNotFoundException(
                                       $"Module instance with id {addModuleInstanceDto.ParentId} not found");

            if (parentModuleName != ModuleConstants.FolderModuleName)
                throw new InvalidOperationException("Only a folder can have child items");
        }

        var position = 0;
        if (db.ModuleInstances.Any())
        {
            if (addModuleInstanceDto.ParentId == null)
            {
                var query = db.ModuleInstances.Where(x => x.ParentId == null);
                position = query.Any() ? await query.MaxAsync(m => m.Order) + 1 : 0;
            }
            else
            {
                var query = db.ModuleInstances.Where(m => m.ParentId == addModuleInstanceDto.ParentId);
                position = query.Any() ? await query.MaxAsync(m => m.Order) + 1 : 0;
            }
        }

        db.ModuleInstances.Add(new ModuleInstanceEntity
        {
            ModuleId = addModuleInstanceDto.ModuleId,
            Label = addModuleInstanceDto.Label,
            Order = position,
            Icon = addModuleInstanceDto.Icon,
            Settings = string.Empty,
            ParentId = addModuleInstanceDto.ParentId
        });
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing module instance’s label and icon.
    /// </summary>
    /// <param name="editModel">The data transfer object containing updated module instance data.</param>
    public async Task EditModuleInstance(EditModuleInstanceDto editModel)
    {
        var instance = await db.ModuleInstances
                           .FirstOrDefaultAsync(x => x.ModuleInstanceId == editModel.ModuleInstanceId)
                       ?? throw new KeyNotFoundException(
                           $"Module instance with id {editModel.ModuleInstanceId} not found");

        instance.Label = editModel.Label;
        instance.Icon = editModel.Icon;

        await db.SaveChangesAsync();

        if (editModel.ViewRoles != null)
            await SetViewRolesAsync(instance.ModuleInstanceId, editModel.ViewRoles);
    }

    /// <summary>
    /// Deletes a module instance from the system by ID.
    /// </summary>
    /// <param name="id">The ID of the module instance to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the module instance does not exist.</exception>
    public async Task DeleteModuleInstance(int id)
    {
        var instance = await db.ModuleInstances.FirstOrDefaultAsync(x => x.ModuleInstanceId == id);
        if (instance == null)
            throw new KeyNotFoundException($"Module instance with id {id} not found");

        await db.ModuleInstances.Where(m => m.ParentId == instance.ParentId && m.Order > instance.Order)
            .ForEachAsync(m => m.Order -= 1);

        var idsToDelete = await GetModuleInstanceTreeIds(id);
        var securitiesToDelete = await db.ModuleInstanceSecurities
            .Where(s => idsToDelete.Contains(s.ModuleInstanceId))
            .ToListAsync();
        var instancesToDelete = await db.ModuleInstances
            .Where(m => idsToDelete.Contains(m.ModuleInstanceId))
            .ToListAsync();

        // The schema carries no foreign keys, so start module references are cleaned up explicitly.
        var startModulesToDelete = await db.UserStartModules
            .Where(s => idsToDelete.Contains(s.ModuleInstanceId))
            .ToListAsync();

        db.ModuleInstanceSecurities.RemoveRange(securitiesToDelete);
        db.UserStartModules.RemoveRange(startModulesToDelete);
        db.ModuleInstances.RemoveRange(instancesToDelete);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Copies a leaf module instance together with its settings and security rules.
    /// The copy is placed under the same parent right below the original.
    /// </summary>
    /// <param name="id">The ID of the module instance to copy.</param>
    /// <returns>The ID of the created module instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the module instance does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the module instance has child items.</exception>
    public async Task<int> CopyModuleInstance(int id)
    {
        var instance = await db.ModuleInstances
                           .FirstOrDefaultAsync(x => x.ModuleInstanceId == id)
                       ?? throw new KeyNotFoundException($"Module instance with id {id} not found");

        if (await db.ModuleInstances.AnyAsync(x => x.ParentId == id))
            throw new InvalidOperationException($"Module instance with id {id} has child items and cannot be copied");

        await db.ModuleInstances.Where(m => m.ParentId == instance.ParentId && m.Order > instance.Order)
            .ForEachAsync(m => m.Order += 1);

        var copy = new ModuleInstanceEntity
        {
            ModuleId = instance.ModuleId,
            Label = $"{instance.Label} (copy)",
            Icon = instance.Icon,
            Url = instance.Url,
            Target = instance.Target,
            ParentId = instance.ParentId,
            Order = instance.Order + 1,
            Settings = instance.Settings
        };

        db.ModuleInstances.Add(copy);
        await db.SaveChangesAsync();

        var acl = await FindAclAsync(id);
        if (acl is not null)
            await Access.SetAclUncheckedAsync(ModuleAccessControl.Instance(copy.ModuleInstanceId), acl.InheritanceBroken,
                acl.Entries.Select(ToAclEntry).ToList());

        return copy.ModuleInstanceId;
    }

    private async Task<HashSet<int>> GetModuleInstanceTreeIds(int rootId)
    {
        var moduleInstances = await db.ModuleInstances
            .Select(x => new { x.ModuleInstanceId, x.ParentId })
            .ToListAsync();
        var childrenByParentId = moduleInstances.ToLookup(x => x.ParentId);
        var idsToDelete = new HashSet<int> { rootId };
        var queue = new Queue<int>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();
            foreach (var childId in childrenByParentId[parentId].Select(x => x.ModuleInstanceId))
            {
                if (idsToDelete.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        return idsToDelete;
    }

    /// <summary>
    /// Deletes a module by its ID.
    /// </summary>
    /// <param name="id">The ID of the module to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the module does not exist.</exception>
    public async Task DeleteModule(int id)
    {
        var module = await db.Modules.FirstOrDefaultAsync(x => x.ModuleId == id);
        if (module == null)
            throw new KeyNotFoundException($"Module with id {id} not found");

        db.Modules.Remove(module);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an extension module by identifier.
    /// </summary>
    public async Task DeleteExtensionModule(int id)
    {
        var module = await db.Modules.FirstOrDefaultAsync(x => x.ModuleId == id && x.Kind == ModuleKind.Extension);
        if (module == null)
            throw new KeyNotFoundException($"Extension module with id {id} not found");

        db.Modules.Remove(module);
        await db.SaveChangesAsync();
    }

    private static ModuleDto ToModuleDto(ModuleEntity module)
    {
        return new ModuleDto
        {
            ModuleId = module.ModuleId,
            Name = module.Name,
            Settings = module.Settings,
            Kind = module.Kind,
            ManifestUrl = module.ManifestUrl,
            ExtensionKey = module.ExtensionKey,
            LoadType = module.LoadType,
            ElementName = module.ElementName,
            ScriptUrl = module.ScriptUrl,
            RemoteEntryUrl = module.RemoteEntryUrl,
            ExposedModule = module.ExposedModule,
            ComponentName = module.ComponentName,
            ApiBaseUrl = module.ApiBaseUrl,
            Version = module.Version,
            ModuleSecurities = module.ModuleSecurities.Select(x => new ModuleSecurityDto
            {
                Right = x.Right,
                Role = x.Role
            })
        };
    }

    /// <summary>
    /// Swaps the display order positions of two module instances.
    /// </summary>
    /// <param name="firstModuleId">Identifier for the first module instance.</param>
    /// <param name="secondModuleId">Identifier for the second module instance.</param>
    /// <exception cref="KeyNotFoundException">Thrown when either module instance ID cannot be found.</exception>
    public async Task ChangeModuleOrder(int firstModuleId, int secondModuleId)
    {
        var firstModule = await db.ModuleInstances.FirstOrDefaultAsync(x => x.ModuleInstanceId == firstModuleId)
                          ?? throw new KeyNotFoundException($"Module instance with id {firstModuleId} not found");
        var secondModule = await db.ModuleInstances.FirstOrDefaultAsync(x => x.ModuleInstanceId == secondModuleId)
                           ?? throw new KeyNotFoundException($"Module instance with id {secondModuleId} not found");

        (firstModule.Order, secondModule.Order) = (secondModule.Order, firstModule.Order);

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the module instance the user opens by default.
    /// </summary>
    /// <param name="userSubject">Subject of the user.</param>
    /// <returns>The module instance id, or <c>null</c> when the user has not chosen one.</returns>
    public async Task<int?> GetStartModuleInstanceId(string? userSubject)
    {
        if (string.IsNullOrWhiteSpace(userSubject))
            return null;

        var startModule = await db.UserStartModules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserSubject == userSubject);

        return startModule?.ModuleInstanceId;
    }

    /// <summary>
    /// Sets the module instance the user opens by default, replacing any previous choice.
    /// </summary>
    /// <param name="userSubject">Subject of the user.</param>
    /// <param name="moduleInstanceId">The module instance to open by default.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the module instance cannot be found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the module instance is a folder.</exception>
    public async Task SetStartModule(string userSubject, int moduleInstanceId)
    {
        var moduleName = await db.ModuleInstances.Where(x => x.ModuleInstanceId == moduleInstanceId)
                             .Select(x => x.Module.Name)
                             .FirstOrDefaultAsync()
                         ?? throw new KeyNotFoundException($"Module instance with id {moduleInstanceId} not found");

        if (moduleName == ModuleConstants.FolderModuleName)
            throw new InvalidOperationException("A folder cannot be a start module");

        // One row per user: older rows are dropped so a partially written state cannot pile up.
        var current = await db.UserStartModules.Where(x => x.UserSubject == userSubject).ToListAsync();
        if (current.Count > 0)
            db.UserStartModules.RemoveRange(current);

        await db.UserStartModules.AddAsync(new UserStartModuleEntity
        {
            UserSubject = userSubject,
            ModuleInstanceId = moduleInstanceId
        });

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Clears the start module instance of the user.
    /// </summary>
    /// <param name="userSubject">Subject of the user.</param>
    public async Task DeleteStartModule(string userSubject)
    {
        var current = await db.UserStartModules.Where(x => x.UserSubject == userSubject).ToListAsync();
        if (current.Count == 0)
            return;

        db.UserStartModules.RemoveRange(current);
        await db.SaveChangesAsync();
    }
}
