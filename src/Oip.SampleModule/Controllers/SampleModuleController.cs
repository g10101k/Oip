using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Oip.SampleModule.Controllers;

/// <summary>
/// Sample module API controller.
/// </summary>
[ApiController]
[Route("api/sample-module")]
[ApiExplorerSettings(GroupName = "v1")]
public class SampleModuleController : ControllerBase
{
    /// <summary>
    /// Returns sample module information.
    /// </summary>
    /// <param name="id">Module instance identifier.</param>
    /// <returns>Sample module information.</returns>
    [Authorize, HttpGet("{id:int}/info")]
    public SampleModuleInfoResponse GetInfo(int id)
    {
        return new SampleModuleInfoResponse(id, "Oip.SampleModule", DateTimeOffset.UtcNow);
    }
}

/// <summary>
/// Sample module information response.
/// </summary>
/// <param name="ModuleInstanceId">Module instance identifier.</param>
/// <param name="Name">Module name.</param>
/// <param name="ServerTimeUtc">Server time in UTC.</param>
public sealed record SampleModuleInfoResponse(int ModuleInstanceId, string Name, DateTimeOffset ServerTimeUtc);
