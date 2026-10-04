using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Exceptions;
using Oip.Hitl.Base.Settings;
using Oip.Hitl.Base.Workflows;
using Oip.Hitl.Controllers.Api;
using Oip.Hitl.Workflows;
using Temporalio.Client;

namespace Oip.Hitl.Controllers;

/// <summary>
/// Demo controller for running Temporal workflows.
/// </summary>
[ApiController]
[Authorize]
[Route("api/workflow-demo")]
public class WorkflowDemoController(
    ITemporalClient client,
    TemporalSettings temporalSettings,
    UserStepService userStepService) : ControllerBase
{
    /// <summary>
    /// Runs <see cref="HelloWorldWorkflow"/> to completion and returns its result.
    /// </summary>
    [HttpPost("run-hello-world")]
    [ProducesResponseType<RunWorkflowResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<RunWorkflowResponse>> RunHelloWorld(CancellationToken cancellationToken)
    {
        return Ok(await client.CallAsync(async () =>
        {
            var rpc = new RpcOptions { CancellationToken = cancellationToken };
            var handle = await client.StartWorkflowAsync((HelloWorldWorkflow wf) => wf.RunAsync(),
                NewWorkflowOptions(nameof(HelloWorldWorkflow), rpc));
            var result = await handle.GetResultAsync(rpcOptions: rpc);
            var description = await handle.DescribeAsync(new WorkflowDescribeOptions { Rpc = rpc });

            return new RunWorkflowResponse
            {
                WorkflowInstanceId = handle.Id,
                Status = description.Status.ToString(),
                Result = result
            };
        }));
    }

    /// <summary>
    /// Starts <see cref="UserTaskDemoWorkflow"/> and returns the link to the page of its first step.
    /// </summary>
    [HttpPost("run-user-task-demo")]
    [ProducesResponseType<RunUserTaskDemoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<RunUserTaskDemoResponse>> RunUserTaskDemo(CancellationToken cancellationToken)
    {
        var handle = await client.CallAsync(() => client.StartWorkflowAsync((UserTaskDemoWorkflow wf) => wf.RunAsync(),
            NewWorkflowOptions(nameof(UserTaskDemoWorkflow), new RpcOptions { CancellationToken = cancellationToken })));
        var step = await userStepService.WaitForFirstStepAsync(handle.Id, cancellationToken);

        return Ok(new RunUserTaskDemoResponse
        {
            WorkflowInstanceId = handle.Id,
            StepId = step?.Id,
            StepUrl = step?.Url
        });
    }

    private WorkflowOptions NewWorkflowOptions(string prefix, RpcOptions rpc) =>
        new($"{prefix}-{Guid.NewGuid():N}", temporalSettings.TaskQueue) { Rpc = rpc };
}
