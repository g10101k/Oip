using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Base.Security;
using Oip.Hil.Base.Controllers.Api;
using Oip.Hil.Base.Workflows;

namespace Oip.Hil.Base.Controllers;

/// <summary>
/// Module that lists the user steps of Temporal workflows started in a period, pending and completed.
/// </summary>
[ApiController]
[Authorize]
[Route("api/workflow-activity-module")]
public class WorkflowActivityModuleController(UserStepService userStepService, ModuleRepository moduleRepository)
    : BaseModuleController<WorkflowActivityModuleSettings>(moduleRepository)
{
    /// <inheritdoc />
    public override string? Icon => "pi pi-sitemap";

    /// <summary>
    /// Returns all steps, pending and completed, of the workflows started in the period, with links to the Angular
    /// pages that render them.
    /// </summary>
    /// <param name="request">Period of the workflow start.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Right(SecurityConstants.Read), HttpPost("get-steps-by-period")]
    [ProducesResponseType<List<UserStepDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<List<UserStepDto>>> GetStepsByPeriod(
        [FromBody] GetStepsByPeriodRequest request, CancellationToken cancellationToken)
    {
        if (request.From > request.To)
            throw new ApiException("Validation error", "The start of the period is after its end",
                StatusCodes.Status400BadRequest);

        return Ok(await userStepService.GetStepsAsync(request.From, request.To, cancellationToken));
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
/// Settings for the WorkflowActivity module.
/// </summary>
public class WorkflowActivityModuleSettings
{
    /// <summary>
    /// Number of days.
    /// </summary>
    public int DayCount { get; set; } = 5;
}
