using Microsoft.AspNetCore.Mvc;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Menu controller
/// </summary>
[ApiController]
[Route("api/menu")]
public class MenuController : Controller
{
    private readonly FeatureRepository _featureRepository;

    /// <summary>
    /// .ctor
    /// </summary>
    public MenuController(FeatureRepository featureRepository)
    {
        _featureRepository = featureRepository;
    }

    /// <summary>
    /// Get menu for client app
    /// </summary>
    /// <returns></returns>
    [HttpGet("get")]
    public async Task<IEnumerable<FeatureInstanceDto>> Get()
    {
        return await _featureRepository.GetFeatureForMenuAll();
    }
}