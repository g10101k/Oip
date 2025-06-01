using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Constants;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Repositories;
using Oip.Base.Extensions;

namespace Oip.Base.Controllers;

/// <summary>
/// Modules controller
/// </summary>
[ApiController]
[Route("api/module")]
public class ModuleController : ControllerBase
{
    private readonly ModuleRepository _moduleRepository;

    /// <summary>
    /// .ctor
    /// </summary>
    public ModuleController(ModuleRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    /// <summary>
    /// Get all modules
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-all")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<IEnumerable<ModuleDto>> GetAll()
    {
        return await _moduleRepository.GetAll();
    }

    /// <summary>
    /// Insert
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    [HttpPost("insert")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task Insert(ModuleDto item)
    {
        await _moduleRepository.Insert([item]);
    }
    
    
    /// <summary>
    /// delete
    /// </summary>
    /// <returns></returns>
    [HttpDelete("delete")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task Delete(ModuleDeleteRequest request)
    {
        await _moduleRepository.DeleteModule(request.ModuleId);
    }
    
    /// <summary>
    /// Returns a list of all registered modules and indicates whether each one is currently loaded into the application.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to users with administrative privileges.
    /// It aggregates module data from the database and compares it against the currently loaded modules in the application context,
    /// returning a combined view with load status flags.
    /// </remarks>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of <see cref="ExistModuleDto"/> objects,
    /// each representing a module and whether it is currently active (loaded).
    /// </returns>
    [HttpGet("get-modules-with-load-status")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<IEnumerable<ModuleDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModulesWithLoadStatus()
    {
        var resultTask = _moduleRepository.GetAll();
        var loadedModulesTask = WebApplicationBuilderExtension.GetAllLoadedModulesAsync();
        await Task.WhenAll(resultTask, loadedModulesTask);
        var (modules, loadedModules) = (resultTask.Result, loadedModulesTask.Result);

        var result = modules.Select(x => new ExistModuleDto()
        {
            ModuleId = x.ModuleId,
            Name = x.Name,
            CurrentlyLoaded = loadedModules.Any(t=>t.Name.Replace("Controller", string.Empty) == x.Name)
        });
        return Ok(result);
    }
}