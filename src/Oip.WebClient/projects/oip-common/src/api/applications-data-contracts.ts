export enum ServiceType {
  Service = "Service",
  Application = "Application",
}

export enum FrontendRemoteEntryKind {
  Routes = "Routes",
  Component = "Component",
}

export enum FrontendIntegrationType {
  InternalRoute = "InternalRoute",
  Iframe = "Iframe",
  FederatedRemote = "FederatedRemote",
  WebComponent = "WebComponent",
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

export interface FrontendRemoteManifestDto {
  code?: string | null;
  title?: string | null;
  type?: FrontendIntegrationType;
  routePath?: string | null;
  remoteEntryUrl?: string | null;
  remoteName?: string | null;
  exposedModule?: string | null;
  entryKind?: FrontendRemoteEntryKind;
  requiredShellVersion?: string | null;
  requiredOipCommonVersion?: string | null;
  angularVersion?: string | null;
  permissions?: string[] | null;
  enabled?: boolean;
}

export interface GetFrontendModuleManifestByCodeParams {
  code: string;
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
