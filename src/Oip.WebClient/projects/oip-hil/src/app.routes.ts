import { Routes } from '@angular/router';
import { oipAuthGuard, provideOipRoutes } from 'oip-common';

/**
 * Routes of the pages that complete a Temporal workflow step: `:workflowId` is the workflow, `:stepId` is its step.
 * They are mounted twice: as children of the workflow activity module (step next to the task list, `embedded` data
 * renders it without its own card) and at the top level (full page linked from the step URL). A new step kind only
 * needs a new entry here.
 */
const workflowStepRoutes: Routes = [
  {
    path: 'workflow-task/:workflowId/:stepId',
    loadComponent: () =>
      import('./app/components/workflow-task/workflow-task.component').then((m) => m.WorkflowTaskComponent)
  },
  {
    path: 'workflow-form/:workflowId/:stepId',
    loadComponent: () =>
      import('./app/components/workflow-form/workflow-form.component').then((m) => m.WorkflowFormComponent)
  },
  {
    path: 'workflow-file-upload/:workflowId/:stepId',
    loadComponent: () =>
      import('./app/components/workflow-file-upload/workflow-file-upload.component').then(
        (m) => m.WorkflowFileUploadComponent
      )
  },
  {
    path: 'workflow-llm-request/:workflowId/:stepId',
    loadComponent: () =>
      import('./app/components/workflow-llm-request/workflow-llm-request.component').then(
        (m) => m.WorkflowLlmRequestComponent
      )
  }
];

export const appRoutes = provideOipRoutes({
  children: [
    {
      path: 'llm-provider-module/:id',
      loadComponent: () =>
        import('./app/components/llm-provider-module/llm-provider-module.component').then(
          (m) => m.LlmProviderModuleComponent
        ),
      canActivate: [oipAuthGuard]
    },
    {
      path: 'agent-module/:id',
      loadComponent: () =>
        import('./app/components/agent-module/agent-module.component').then((m) => m.AgentModuleComponent),
      canActivate: [oipAuthGuard]
    },
    {
      path: 'workflow-activity-module/:id',
      loadComponent: () =>
        import('./app/components/workflow-activity-module/workflow-activity-module.component').then(
          (m) => m.WorkflowActivityModuleComponent
        ),
      canActivate: [oipAuthGuard],
      children: workflowStepRoutes.map((route) => ({ ...route, data: { embedded: true } }))
    },
    ...workflowStepRoutes.map((route) => ({ ...route, canActivate: [oipAuthGuard] }))
  ]
});
