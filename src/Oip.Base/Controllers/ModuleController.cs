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
/// API controller for managing modules in the system.
/// </summary>
[ApiController]
[Route("api/module")]
[ApiExplorerSettings(GroupName = "base")]
public class ModuleController(ModuleRepository moduleRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves all modules stored in the system.
    /// </summary>
    /// <remarks>
    /// Only accessible to users with the Admin role.
    /// </remarks>
    /// <returns>A list of <see cref="ModuleDto"/> objects representing all modules.</returns>
    [HttpGet("get-all")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<IEnumerable<ModuleDto>> GetAll()
    {
        return await moduleRepository.GetAll();
    }

    /// <summary>
    /// Inserts a new module into the system.
    /// </summary>
    /// <param name="item">The module data to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpPost("insert")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task Insert(ModuleDto item)
    {
        await moduleRepository.Insert([item]);
    }

    /// <summary>
    /// Deletes a module by its identifier.
    /// </summary>
    /// <param name="request">The request object containing the module ID to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpDelete("delete")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task Delete(ModuleDeleteRequest request)
    {
        await moduleRepository.DeleteModule(request.ModuleId);
    }

    /// <summary>
    /// Returns all registered modules and indicates whether each one is currently loaded into the application.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of <see cref="ExistModuleDto"/> objects,
    /// each representing a module with its load status.
    /// </returns>
    [HttpGet("get-modules-with-load-status")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<IEnumerable<ExistModuleDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModulesWithLoadStatus()
    {
        var resultTask = moduleRepository.GetAll();
        var loadedModulesTask = WebApplicationBuilderExtension.GetAllLoadedModulesAsync();
        await Task.WhenAll(resultTask, loadedModulesTask);

        var (modules, loadedModules) = (resultTask.Result, loadedModulesTask.Result);

        var result = modules.Select(x => new ExistModuleDto()
        {
            ModuleId = x.ModuleId,
            Name = x.Name,
            CurrentlyLoaded = loadedModules.Any(t => t.Name.Replace("Controller", string.Empty) == x.Name)
        });

        return Ok(result);
    }
}