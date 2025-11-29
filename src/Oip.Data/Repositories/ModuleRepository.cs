using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Entities;

namespace Oip.Base.Data.Repositories;

/// <summary>
/// Provides methods to manage modules and their instances, including CRUD operations and security settings.
/// </summary>
/// <remarks>
/// This repository encapsulates access to module data, such as module definitions, settings, instances, and associated security rules.
/// </remarks>
public class ModuleRepository
{
    private readonly OipModuleContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleRepository"/> class.
    /// </summary>
    /// <param name="db">The database context for module operations.</param>
    public ModuleRepository(OipModuleContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves all available modules with their associated security settings.
    /// </summary>
    /// <return>A collection of module DTOs.</return>
    public async Task<IEnumerable<ModuleDto>> GetAll()
    {
        var query = from module in _db.Modules
            join moduleSecurity in _db.ModuleSecurities on module.ModuleId equals moduleSecurity.ModuleId into
                security
            select new ModuleDto()
            {
                ModuleId = module.ModuleId,
                Settings = module.Settings,
                Name = module.Name,
                ModuleSecurities = security.Select(x => new ModuleSecurityDto()
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
        await _db.Modules.AddRangeAsync(list.Select(x =>
            new ModuleEntity
            {
                Name = x.Name,
                Settings = x.Settings,
                ModuleSecurities = x.ModuleSecurities.Select(xx => new ModuleSecurityEntity()
                {
                    ModuleId = x.ModuleId,
                    Right = xx.Right,
                    Role = xx.Role
                }).ToList()
            }
        ));
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all module instances available in the menu, filtered by user roles.
    /// </summary>
    /// <param name="roles">List of user roles to filter modules by access.</param>
    /// <returns>A collection of module instances accessible to the specified roles.</returns>
    public async Task<IEnumerable<ModuleInstanceDto>> GetModuleForMenuAll(List<string> roles)
    {
        var query = await _db.ModuleInstances
            .Include(x => x.Module)
            .Include(x => x.Securities)
            .Where(m => m.Securities
                .Any(s => s.Right == "read" && roles.Contains(s.Role)))
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
        var query = from security in _db.ModuleInstanceSecurities
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
            from sec in _db.ModuleInstanceSecurities
            where sec.ModuleInstanceId == id
            select sec;
        var list = await query.ToListAsync();
        SyncRule(id, security, list);

        await _db.SaveChangesAsync();
    }

    private void SyncRule(int id, IEnumerable<ModuleSecurityDto> security, List<ModuleInstanceSecurityEntity> list)
    {
        // delete
        var listToDelete = list.Where(x => !security.Any(s => s.Right == x.Right && s.Role == x.Role)).ToList();
        _db.ModuleInstanceSecurities.RemoveRange(listToDelete);
        // add
        var listToAdd = security.Where(x => !list.Exists(s => s.Right == x.Right && s.Role == x.Role))
            .Select(x => new ModuleInstanceSecurityEntity()
            {
                ModuleInstanceId = id,
                Right = x.Right,
                Role = x.Role
            }).ToList();
        _db.ModuleInstanceSecurities.AddRange(listToAdd);
    }

    /// <summary>
    /// Converts a <see cref="ModuleInstanceEntity"/> to a <see cref="ModuleInstanceDto"/>.
    /// </summary>
    /// <param name="module">The <see cref="ModuleInstanceEntity"/> to convert.</param>
    /// <return>The converted <see cref="ModuleInstanceDto"/>.</return>
    private static ModuleInstanceDto ToDto(ModuleInstanceEntity module)
    {
        return new ModuleInstanceDto()
        {
            ModuleInstanceId = module.ModuleInstanceId,
            ModuleId = module.ModuleId,
            Label = module.Label,
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
        var settings = _db.ModuleInstances.Where(x => x.ModuleInstanceId == id).Select(x => x.Settings)
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
        var moduleInstance = _db.ModuleInstances.FirstOrDefault(x => x.ModuleInstanceId == id);

        if (moduleInstance == null)
            throw new KeyNotFoundException($"Module instance with id {id} not found");

        moduleInstance.Settings = settings;

        _db.SaveChanges();
    }

    /// <summary>
    /// Retrieves the full list of module instances accessible by administrators.
    /// </summary>
    /// <returns>A collection of all top-level module instances with admin access.</returns>
    public async Task<IEnumerable<ModuleInstanceDto>> GetAdminMenu()
    {
        var query = from module in _db.ModuleInstances
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
        var query = from module in _db.Modules
            select new IntKeyValueDto(module.ModuleId, module.Name);
        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Adds a new module instance to the system with default security settings.
    /// </summary>
    /// <param name="addModuleInstanceDto">The data transfer object containing module instance details.</param>
    public async Task AddModuleInstance(AddModuleInstanceDto addModuleInstanceDto)
    {
        _db.ModuleInstances.Add(new ModuleInstanceEntity()
        {
            ModuleId = addModuleInstanceDto.ModuleId,
            Label = addModuleInstanceDto.Label,
            Icon = addModuleInstanceDto.Icon,
            Settings = string.Empty,
            ParentId = addModuleInstanceDto.ParentId,
            Securities = [new ModuleInstanceSecurityEntity { Right = "read", Role = "admin" }]
        });
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing module instance’s label and icon.
    /// </summary>
    /// <param name="editModel">The data transfer object containing updated module instance data.</param>
    public async Task EditModuleInstance(EditModuleInstanceDto editModel)
    {
        var instance = await _db.ModuleInstances
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
                _db.ModuleInstanceSecurities.Remove(roleEntity);
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
                _db.ModuleInstanceSecurities.Add(newRole);
            }
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a module instance from the system by ID.
    /// </summary>
    /// <param name="id">The ID of the module instance to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the module instance does not exist.</exception>
    public async Task DeleteModuleInstance(int id)
    {
        var instance = await _db.ModuleInstances.FirstOrDefaultAsync(x => x.ModuleInstanceId == id);
        if (instance == null)
            throw new KeyNotFoundException($"Module instance with id {id} not found");

        _db.ModuleInstances.Remove(instance);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a module by its ID.
    /// </summary>
    /// <param name="id">The ID of the module to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the module does not exist.</exception>
    public async Task DeleteModule(int id)
    {
        var module = await _db.Modules.FirstOrDefaultAsync(x => x.ModuleId == id);
        if (module == null)
            throw new KeyNotFoundException($"Module with id {id} not found");

        _db.Modules.Remove(module);
        await _db.SaveChangesAsync();
    }
}