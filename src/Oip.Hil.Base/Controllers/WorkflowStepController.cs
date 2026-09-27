using Microsoft.AspNetCore.Mvc;
using Oip.Hil.Base.Workflows;

namespace Oip.Hil.Base.Controllers;

/// <summary>
/// Serves the Angular pages of workflow user steps.
/// </summary>
[Route("api/workflow-step")]
public class WorkflowStepController(UserStepService userStepService) : BaseWorkflowStepController(userStepService);
