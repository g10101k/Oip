using Microsoft.AspNetCore.Mvc;
using Oip.Data.Dtos;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Features controller
/// </summary>
[ApiController]
[Route("api/feature")]
public class FeatureController : ControllerBase
{
    private readonly FeatureRepository _featureRepository;

    /// <summary>
    /// .ctor
    /// </summary>
    public FeatureController(FeatureRepository featureRepository)
    {
        _featureRepository = featureRepository;
    }

    /// <summary>
    /// Get all features
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-all")]
    public async Task<IEnumerable<FeatureDto>> GetAll()
    {
        return await _featureRepository.GetAll();
    }

    /// <summary>
    /// Insert
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    [HttpPost("insert")]
    public async Task Insert(FeatureDto item)
    {
        await _featureRepository.Insert(new[] { item });
    }
}