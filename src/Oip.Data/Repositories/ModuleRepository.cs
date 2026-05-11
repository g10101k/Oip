using Microsoft.EntityFrameworkCore;
using Oip.Data.Contexts;
using Oip.Data.Dtos;
using Oip.Data.Entities;
using Oip.Data.Extensions;

namespace Oip.Data.Repositories;

/// <summary>
/// Provides methods to manage modules and their instances, including CRUD operations and security settings.
/// </summary>
/// <remarks>
/// This repository encapsulates access to module data, such as module definitions, settings, instances, and associated security rules.
/// </remarks>
public class ModuleRepository(OipModuleContext db)
{
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
                ElementName = module.ElementName,
                ScriptUrl = module.ScriptUrl,
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
                ElementName = x.ElementName,
                ScriptUrl = x.ScriptUrl,
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
    /// <returns>A collection of module instances accessible to the specified roles.</returns>
    public async Task<IEnumerable<ModuleInstanceDto>> GetModuleForMenuAll(List<string> roles)
    {
        var loadedModules = await WebApplicationBuilderExtension.GetAllLoadedModulesAsync();
        var modulesLoadedNames = loadedModules.Select(x => x.Name.Replace("Controller", string.Empty)).ToList();
        var query = await db.ModuleInstances
            .Include(x => x.Module)
            .Where(x => x.Module.Kind == ModuleKind.Extension || modulesLoadedNames.Contains(x.Module.Name)) // Загружаем связанный Module
            .Include(x => x.Securities) // Загружаем Securities (если нужно)
            .Where(m => m.Securities.Any(s => s.Right == "read" && roles.Contains(s.Role)))
            .OrderBy(m => m.Order)
            .ToListAsync();

        var result = query.Where(x => x.Parent == null).Select(ToDto);

        return result.ToList();
    }

    /// <summary>
    /// Retrieves the list of security settings associated with a specific module instance.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <returns>A list of security roles and rights for the given module instance.</returns>
    public async Task<List<ModuleSecurityDto>> GetSecurityByInstanceId(int id)
    {
        var query = from security in db.ModuleInstanceSecurities
            where security.ModuleInstanceId == id
            select new ModuleSecurityDto
            {
                Right = security.Right,
                Role = security.Role
            };

        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Updates the security rules for a given module instance.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <param name="security">The updated list of security roles and rights.</param>
    public async Task UpdateInstanceSecurity(int id, IEnumerable<ModuleSecurityDto> security)
    {
        var query =
            from sec in db.ModuleInstanceSecurities
            where sec.ModuleInstanceId == id
            select sec;
        var list = await query.ToListAsync();
        SyncRule(id, security, list);

        await db.SaveChangesAsync();
    }

    private void SyncRule(int id, IEnumerable<ModuleSecurityDto> security, List<ModuleInstanceSecurityEntity> list)
    {
        // delete
        var listToDelete = list.Where(x => !security.Any(s => s.Right == x.Right && s.Role == x.Role)).ToList();
        db.ModuleInstanceSecurities.RemoveRange(listToDelete);
        // add
        var listToAdd = security.Where(x => !list.Exists(s => s.Right == x.Right && s.Role == x.Role))
            .Select(x => new ModuleInstanceSecurityEntity()
            {
                ModuleInstanceId = id,
                Right = x.Right,
                Role = x.Role
            }).ToList();
        db.ModuleInstanceSecurities.AddRange(listToAdd);
    }

    /// <summary>
    /// Converts a <see cref="ModuleInstanceEntity"/> to a <see cref="ModuleInstanceDto"/>.
    /// </summary>
    /// <param name="module">The <see cref="ModuleInstanceEntity"/> to convert.</param>
    /// <return>The converted <see cref="ModuleInstanceDto"/>.</return>
    private static ModuleInstanceDto ToDto(ModuleInstanceEntity module)
    {
        return new ModuleInstanceDto
        {
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
            Items = module.Items.Count == 0 ? null : module.Items.Select(ToDto).ToList(),
            Securities = module.Securities.Select(s => s.Role).ToList()
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
    /// <returns>The settings string.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the module instance is not found.</exception>
    public string GetModuleInstanceSettings(int id)
    {
        var settings = db.ModuleInstances.Where(x => x.ModuleInstanceId == id).Select(x => x.Settings)
            .FirstOrDefault();

        if (settings == null)
            throw new KeyNotFoundException($"Module instance with id {id} not found");
        return settings;
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
        var result = (await query.ToListAsync()).Where(x => x.Parent == null).Select(ToDto);

        return result.ToList();
    }

    /// <summary>
    /// Retrieves a list of all modules as key-value pairs.
    /// </summary>
    /// <returns>A list of modules with ID and name.</returns>
    public async Task<IEnumerable<IntKeyValueDto>> GetModules()
    {
        var query = from module in db.Modules
            select new IntKeyValueDto(module.ModuleId, module.Name);
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
                ElementName = module.ElementName,
                ScriptUrl = module.ScriptUrl,
                ApiBaseUrl = module.ApiBaseUrl,
                Version = module.Version,
                ModuleSecurities = module.ModuleSecurities.Select(x => new ModuleSecurityDto
                {
                    Right = x.Right,
                    Role = x.Role
                })
            };

        return await query.AsNoTracking().FirstOrDefaultAsync();
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
            Kind = ModuleKind.Extension,
            ManifestUrl = manifestUrl,
            ExtensionKey = manifest.Key,
            ElementName = manifest.ElementName,
            ScriptUrl = manifest.ScriptUrl,
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
    public async Task<ModuleDto> UpdateExtensionModule(int moduleId, ExtensionModuleManifestDto manifest, string manifestUrl)
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
        module.ManifestUrl = manifestUrl;
        module.ExtensionKey = manifest.Key;
        module.ElementName = manifest.ElementName;
        module.ScriptUrl = manifest.ScriptUrl;
        module.ApiBaseUrl = manifest.ApiBaseUrl;
        module.Version = manifest.Version;

        await db.SaveChangesAsync();
        return ToModuleDto(module);
    }

    /// <summary>
    /// Adds a new module instance to the system with default security settings.
    /// </summary>
    /// <param name="addModuleInstanceDto">The data transfer object containing module instance details.</param>
    public async Task AddModuleInstance(AddModuleInstanceDto addModuleInstanceDto)
    {
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
            ParentId = addModuleInstanceDto.ParentId,
            Securities = [new ModuleInstanceSecurityEntity { Right = "read", Role = "admin" }]
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
                           .Include(x => x.Securities)
                           .FirstOrDefaultAsync(x => x.ModuleInstanceId == editModel.ModuleInstanceId)
                       ?? throw new KeyNotFoundException(
                           $"Module instance with id {editModel.ModuleInstanceId} not found");

        instance.Label = editModel.Label;
        instance.Icon = editModel.Icon;

        if (editModel.ViewRoles != null)
        {
            var rolesToRemove = instance.Securities
                .Where(s => !editModel.ViewRoles.Contains(s.Role))
                .ToList();

            foreach (var roleEntity in rolesToRemove)
            {
                db.ModuleInstanceSecurities.Remove(roleEntity);
            }

            var existingRoles = instance.Securities.Select(s => s.Role).ToHashSet();
            var newRoles = editModel.ViewRoles
                .Where(role => !existingRoles.Contains(role))
                .Select(role => new ModuleInstanceSecurityEntity
                {
                    ModuleInstance = instance,
                    ModuleInstanceId = instance.ModuleInstanceId,
                    Role = role,
                    Right = "read"
                });

            foreach (var newRole in newRoles)
            {
                db.ModuleInstanceSecurities.Add(newRole);
            }
        }

        await db.SaveChangesAsync();
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

        db.ModuleInstances.Remove(instance);
        await db.SaveChangesAsync();
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
            ElementName = module.ElementName,
            ScriptUrl = module.ScriptUrl,
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
}
