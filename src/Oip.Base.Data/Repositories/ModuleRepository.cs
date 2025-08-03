using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Entities;

namespace Oip.Base.Data.Repositories;

/// <summary>
/// Module Repository
/// </summary>
public class ModuleRepository
{
    private readonly OipModuleContext _db;

    /// <summary>
    /// .ctor
    /// </summary>
    public ModuleRepository(OipModuleContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all modules.
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
    /// Insert module
    /// </summary>
    /// <param name="list">The list of module to insert.</param>
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
    /// Get all
    /// </summary>
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
    /// Get instance security by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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
    /// Update
    /// </summary>
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
    /// Get setting by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public string GetModuleInstanceSettings(int id)
    {
        var settings = _db.ModuleInstances.Where(x => x.ModuleInstanceId == id).Select(x => x.Settings)
            .FirstOrDefault();

        if (settings == null)
            throw new KeyNotFoundException($"Module instance with id {id} not found");
        return settings;
    }

    /// <summary>
    /// Update module instance settings
    /// </summary>
    /// <param name="id"></param>
    /// <param name="settings"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public void UpdateModuleInstanceSettings(int id, string settings)
    {
        var moduleInstance = _db.ModuleInstances.FirstOrDefault(x => x.ModuleInstanceId == id);

        if (moduleInstance == null)
            throw new KeyNotFoundException($"Module instance with id {id} not found");

        moduleInstance.Settings = settings;

        _db.SaveChanges();
    }

    /// <summary>
    /// Gets all top-level module instances for the admin menu.
    /// </summary>
    /// <return>An enumerable collection of ModuleInstanceDto objects representing the admin menu.</return>
    public async Task<IEnumerable<ModuleInstanceDto>> GetAdminMenu()
    {
        var query = from module in _db.ModuleInstances
                .Include(x => x.Module)
            select module;
        var result = (await query.ToListAsync()).Where(x => x.Parent == null).Select(ToDto);

        return result.ToList();
    }

    /// <summary>
    /// Get modules
    /// </summary>
    /// <returns>A collection of integer key-value pairs representing modules.</returns>
    public async Task<IEnumerable<IntKeyValueDto>> GetModules()
    {
        var query = from module in _db.Modules
            select new IntKeyValueDto(module.ModuleId, module.Name);
        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Add module instance
    /// </summary>
    /// <param name="addModuleInstanceDto"></param>
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
    /// Edit module instance
    /// </summary>
    /// <param name="editModel"></param>
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
    /// Delete module instance
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteModuleInstance(int id)
    {
        var instance = await _db.ModuleInstances.FirstOrDefaultAsync(x => x.ModuleInstanceId == id);
        if (instance == null)
            throw new KeyNotFoundException($"Module instance with id {id} not found");

        _db.ModuleInstances.Remove(instance);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Delete module
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteModule(int id)
    {
        var module = await _db.Modules.FirstOrDefaultAsync(x => x.ModuleId == id);
        if (module == null)
            throw new KeyNotFoundException($"Module with id {id} not found");

        _db.Modules.Remove(module);
        await _db.SaveChangesAsync();
    }
}