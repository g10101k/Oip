using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Exceptions;
using Oip.Hil.Base.Controllers.Api;
using Oip.Hil.Base.Workflows;

namespace Oip.Hil.Base.Controllers;

/// <summary>
/// Base controller for the Angular pages of workflow user steps: reads a step and completes it.
/// The application derives from it and sets the route, so the endpoints exist only where workflows are hosted.
/// </summary>
[ApiController]
[Authorize]
public abstract class BaseWorkflowStepController(UserStepService userStepService) : ControllerBase
{
    /// <summary>
    /// Returns a pending step of the workflow by its id.
    /// </summary>
    [HttpGet("get-step-by-id/{workflowId}/{stepId}")]
    [ProducesResponseType<UserStepDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<UserStepDto>> GetStepById(string workflowId, string stepId,
        CancellationToken cancellationToken)
    {
        return Ok(await userStepService.GetByIdAsync(workflowId, stepId, cancellationToken));
    }

    /// <summary>
    /// Completes the step and resumes its workflow.
    /// </summary>
    [HttpPost("complete-step/{workflowId}/{stepId}")]
    [ProducesResponseType<CompleteUserStepResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CompleteUserStepResponse>> CompleteStep(string workflowId,
        string stepId, [FromBody] CompleteUserStepRequest request, CancellationToken cancellationToken)
    {
        return Ok(await userStepService.CompleteAsync(workflowId, stepId, request, GetUserName(), cancellationToken));
    }

    /// <summary>
    /// Saves the file to the step folder and completes the step with it.
    /// </summary>
    [HttpPost("upload-step-file/{workflowId}/{stepId}")]
    [ProducesResponseType<CompleteUserStepResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CompleteUserStepResponse>> UploadStepFile(string workflowId, string stepId,
        [FromForm] UploadStepFileRequest request, CancellationToken cancellationToken)
    {
        return Ok(await userStepService.UploadAsync(workflowId, stepId, request.File, GetUserName(),
            cancellationToken));
    }

    /// <summary>
    /// Downloads a file attached to the step.
    /// </summary>
    [HttpGet("get-step-attachment-by-name/{workflowId}/{stepId}/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetStepAttachmentByName(string workflowId, string stepId, string fileName,
        CancellationToken cancellationToken)
    {
        var (attachment, content) =
            await userStepService.GetAttachmentAsync(workflowId, stepId, fileName, cancellationToken);
        return File(content, attachment.ContentType, attachment.FileName);
    }

    /// <summary>
    /// The claims transformation adds the user name to a secondary identity, so User.Identity.Name may be empty.
    /// </summary>
    private string? GetUserName() => User.FindFirstValue("preferred_username") ?? User.Identity?.Name;
}
