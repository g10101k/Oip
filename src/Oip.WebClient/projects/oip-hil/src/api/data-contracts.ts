export enum UserStepStatus {
  Pending = "Pending",
  Completed = "Completed",
  Cancelled = "Cancelled",
  Running = "Running",
  Failed = "Failed",
}

export enum LlmProviderType {
  OpenAi = "OpenAi",
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

export interface GetLlmProviderModelsRequest {
  baseUrl?: string | null;
  apiKey?: string | null;
  providerId?: number | null;
}

export interface GetStepsByPeriodRequest {
  from?: Date;
  to?: Date;
}

export interface LlmProviderDto {
  id?: number;
  name?: string | null;
  providerType?: LlmProviderType;
  baseUrl?: string | null;
  model?: string | null;
  hasApiKey?: boolean;
  apiKeyHint?: string | null;
  isDefault?: boolean;
  isEnabled?: boolean;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface LlmProviderModuleSettings {
  showDisabled?: boolean;
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

export interface SaveLlmProviderRequest {
  name?: string | null;
  baseUrl?: string | null;
  model?: string | null;
  apiKey?: string | null;
  providerType?: LlmProviderType;
  isDefault?: boolean;
  isEnabled?: boolean;
}

export interface TestLlmProviderResponse {
  success?: boolean;
  message?: string | null;
  elapsedMs?: number;
  modelsCount?: number | null;
  modelFound?: boolean | null;
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
  error?: string | null;
  attachments?: WorkflowAttachment[] | null;
}

export interface WorkflowActivityModuleSettings {
  dayCount?: number;
}

export interface WorkflowAttachment {
  stepId?: string | null;
  fileName?: string | null;
  contentType?: string | null;
  size?: number;
}

export interface UpdateProviderParams {
  id: number;
}

export interface DeleteProviderParams {
  id: number;
}

export interface TestProviderParams {
  id: number;
}

export interface GetStepByIdParams {
  workflowId: string;
  stepId: string;
}

export interface CompleteStepParams {
  workflowId: string;
  stepId: string;
}

export interface UploadStepFilePayload {
  File: Blob;
}

export interface UploadStepFileParams {
  workflowId: string;
  stepId: string;
}

export interface GetStepAttachmentByNameParams {
  workflowId: string;
  stepId: string;
  fileName: string;
}
