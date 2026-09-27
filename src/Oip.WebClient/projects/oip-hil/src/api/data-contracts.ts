export enum UserStepStatus {
  Pending = "Pending",
  Completed = "Completed",
  Cancelled = "Cancelled",
}

export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export interface CompleteUserStepRequest {
  result?: string | null;
  comment?: string | null;
}

export interface CompleteUserStepResponse {
  workflowInstanceId: string | null;
  status: string | null;
}

export interface GetStepsByPeriodRequest {
  from?: Date;
  to?: Date;
}

export interface RunUserTaskDemoResponse {
  workflowInstanceId: string | null;
  stepId?: string | null;
  stepUrl?: string | null;
}

export interface RunWorkflowResponse {
  workflowInstanceId: string | null;
  status: string | null;
  result?: string | null;
}

export interface UserStepDto {
  id: string | null;
  workflowInstanceId: string | null;
  route: string | null;
  url: string | null;
  title: string | null;
  description?: string | null;
  data?: string | null;
  createdAt?: Date;
  status?: UserStepStatus;
  workflowStatus: string | null;
  completedAt?: Date | null;
  completedBy?: string | null;
  comment?: string | null;
  result?: string | null;
}

export interface WorkflowActivityModuleSettings {
  dayCount?: number;
}

export interface GetStepByIdParams {
  workflowId: string;
  stepId: string;
}

export interface CompleteStepParams {
  workflowId: string;
  stepId: string;
}
