using Microsoft.AspNetCore.Mvc;
using Oip.Data.Dtos;
using Oip.Data.Repositories;

namespace Oip.Controllers;

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
    public async Task Insert(ModuleDto item)
    {
        await _moduleRepository.Insert([item]);
    }
}