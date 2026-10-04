using Microsoft.AspNetCore.Mvc;
using Oip.Hitl.Base.Workflows;

namespace Oip.Hitl.Base.Controllers;

/// <summary>
/// Serves the Angular pages of workflow user steps.
/// </summary>
[Route("api/workflow-step")]
public class WorkflowStepController(UserStepService userStepService) : BaseWorkflowStepController(userStepService);
