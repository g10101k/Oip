export interface AddModuleInstanceDto {
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  parentId?: number | null;
  viewRoles?: string[] | null;
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
  apiBaseUrl?: string | null;
  icon?: string | null;
  order?: number;
  enabled?: boolean;
  isCurrent?: boolean;
}

export interface AuthCsrfTokenResponse {
  token?: string | null;
  headerName?: string | null;
}

export interface AuthSessionResponse {
  isAuthenticated?: boolean;
  userName?: string | null;
  displayName?: string | null;
  email?: string | null;
  roles?: string[] | null;
}

export interface CryptRequest {
  message?: string | null;
}

export interface EditModuleInstanceDto {
  moduleInstanceId?: number;
  label?: string | null;
  icon?: string | null;
  parentId?: number | null;
  viewRoles?: string[] | null;
  moduleId?: number | null;
}

export interface ExistModuleDto {
  moduleId?: number;
  name?: string | null;
  currentlyLoaded?: boolean;
}

export interface FolderModuleSettings {
  html?: string | null;
}

export interface GetKeycloakClientSettingsResponse {
  authority?: string | null;
  clientId?: string | null;
  scope?: string | null;
  responseType?: string | null;
  useRefreshToken?: boolean;
  silentRenew?: boolean;
  logLevel?: number;
  secureRoutes?: string[] | null;
}

export interface IframeModuleSettings {
  url?: string | null;
}

export interface IntKeyValueDto {
  key?: number;
  value?: string | null;
}

export interface ModuleDeleteRequest {
  moduleId?: number;
}

export interface ModuleDto {
  moduleId?: number;
  name?: string | null;
  settings?: string | null;
  moduleSecurities?: ModuleSecurityDto[] | null;
}

export interface ModuleInstanceDto {
  moduleInstanceId?: number;
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  routerLink?: string[] | null;
  url?: string | null;
  target?: string | null;
  settings?: string | null;
  items?: ModuleInstanceDto[] | null;
  securities?: string[] | null;
  parentId?: number | null;
  order?: number;
  separator?: boolean;
}

export interface ModuleSecurityDto {
  right: string | null;
  role: string | null;
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

export interface GetModuleInstanceSettingsParams {
  id?: number;
}

export interface GetModuleInstanceSettingsParams2 {
  id?: number;
}

export interface DeleteModuleInstanceParams {
  id?: number;
}

export interface ChangeOrderParams {
  firstModuleId?: number;
  secondModuleId?: number;
}
