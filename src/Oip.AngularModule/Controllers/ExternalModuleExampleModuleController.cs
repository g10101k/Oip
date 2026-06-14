using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;

namespace Oip.AngularModule.Controllers;

/// <summary>
/// Controller for the ExternalModuleExample module.
/// </summary>
[ApiController]
[Route("api/external-module-example-module")]
public class ExternalModuleExampleModuleController(ModuleRepository moduleRepository)
    : BaseModuleController<ExternalModuleExampleModuleSettings>(moduleRepository)
{
    /// <summary>
    /// Retrieves example data from backend.
    /// </summary>
    [HttpGet("get-external-module-example-data")]
    [Authorize]
    [ProducesResponseType<ExternalModuleExampleDataDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public IActionResult GetExternalModuleExampleData()
    {
        return Ok(new ExternalModuleExampleDataDto
        {
            Message = "Data loaded from ASP.NET backend",
            GeneratedAt = DateTimeOffset.UtcNow,
            Items =
            [
                "Backend endpoint is available",
                "Angular generated client can call it",
                "External module renders received data"
            ]
        });
    }

    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return
        [
            new SecurityResponse
            {
                Code = SecurityConstants.Read,
                Name = "Read",
                Description = "Can view this module",
                Roles = [SecurityConstants.AdminRole]
            }
        ];
    }
}

/// <summary>
/// Settings for the ExternalModuleExample module.
/// </summary>
public class ExternalModuleExampleModuleSettings
{
}

/// <summary>
/// Example backend data for the ExternalModuleExample module.
/// </summary>
public class ExternalModuleExampleDataDto
{
    /// <summary>
    /// Example message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Data generation date and time.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }

    /// <summary>
    /// Example items.
    /// </summary>
    public List<string> Items { get; set; } = [];
}
