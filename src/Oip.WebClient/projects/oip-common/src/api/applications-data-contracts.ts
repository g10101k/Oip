export enum ServiceType {
  Service = "Service",
  Application = "Application",
}

export interface ApiExceptionResponse {
  title?: string | null;
  message?: string | null;
  statusCode?: number;
  stackTrace?: string | null;
}

export interface ApplicationRegistryItemDto {
  code?: string | null;
  displayName?: string | null;
  baseUrl?: string | null;
  internalBaseUrl?: string | null;
  icon?: string | null;
  order?: number;
  enabled?: boolean;
  serviceType?: ServiceType;
  isCurrent?: boolean;
}

export interface GetApplicationRegistryItemByCodeParams {
  code: string;
}

export interface UpdateApplicationRegistryItemParams {
  code: string;
}

export interface DeleteApplicationRegistryItemParams {
  code: string;
}
