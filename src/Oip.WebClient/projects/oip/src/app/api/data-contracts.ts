/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

/** DTO for create module instance */
export interface AddModuleInstanceDto {
  /** @format int32 */
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  /** @format int32 */
  parentId?: number | null;
  viewRoles?: string[] | null;
}

/** Apply Migration Request */
export interface ApplyMigrationRequest {
  /** Migration name */
  name?: string | null;
}

/** Settings */
export interface DashboardSettings {
  /** Just for example */
  nothing?: string | null;
}

/** Save settings request */
export interface DashboardSettingsSaveSettingsRequest {
  /**
   * Module instance id
   * @format int32
   */
  id?: number;
  /** Settings */
  settings?: DashboardSettings;
}

/** DTO for edit module instance */
export interface EditModuleInstanceDto {
  /** @format int32 */
  moduleInstanceId?: number;
  label?: string | null;
  icon?: string | null;
  /** @format int32 */
  parentId?: number | null;
  viewRoles?: string[] | null;
}

/** Response front security settings */
export interface GetKeycloakClientSettingsResponse {
  /** Authority */
  authority?: string | null;
  /** Client id */
  clientId?: string | null;
  /** Scope */
  scope?: string | null;
  /** Response Type */
  responseType?: string | null;
  /** Use Refresh Token */
  useRefreshToken?: boolean;
  /** Silent Renew */
  silentRenew?: boolean;
  /**
   * Log level None = 0, Debug = 1, Warn = 2, Error = 3
   * @format int32
   */
  logLevel?: number;
  /** Urls with auth */
  secureRoutes?: string[] | null;
}

/** Response for module federation */
export interface GetManifestResponse {
  /** Base Url */
  baseUrl?: string | null;
}

/** Int Key Value DTO */
export interface IntKeyValueDto {
  /** @format int32 */
  key?: number;
  value?: string | null;
}

/** Модель миграции */
export interface MigrationDto {
  name?: string | null;
  applied?: boolean;
  pending?: boolean;
  exist?: boolean;
}

/** Represents a request to delete a module by its identifier. */
export interface ModuleDeleteRequest {
  /**
   * Gets or sets the unique identifier of the module to be deleted.
   * @format int32
   */
  moduleId?: number;
}

/** It module in app */
export interface ModuleDto {
  /**
   * Id
   * @format int32
   */
  moduleId?: number;
  /** Name */
  name?: string | null;
  /** Settings */
  settings?: string | null;
  /** Securities */
  moduleSecurities?: ModuleSecurityDto[] | null;
}

/** Module Instance Dto */
export interface ModuleInstanceDto {
  /** @format int32 */
  moduleInstanceId?: number;
  /** @format int32 */
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  routerLink?: string[] | null;
  url?: string | null;
  target?: string | null;
  settings?: string | null;
  /** Childs */
  items?: ModuleInstanceDto[] | null;
}

/** Module security DTO */
export interface ModuleSecurityDto {
  /** Right */
  right?: string | null;
  /** Role */
  role?: string | null;
}

/** Save settings request */
export interface ObjectSaveSettingsRequest {
  /**
   * Module instance id
   * @format int32
   */
  id?: number;
  /** Settings */
  settings?: any;
}

/** Put security dto */
export interface PutSecurityRequest {
  /**
   * Instance id
   * @format int32
   */
  id?: number;
  /** Securities */
  securities?: SecurityResponse[] | null;
}

/** Dto module */
export interface RegisterModuleDto {
  /** See 'name' in webpack.config.js */
  name?: string | null;
  /** Base Url */
  baseUrl?: string | null;
}

/** Security dto */
export interface SecurityResponse {
  /** Code */
  code?: string | null;
  /** Name */
  name?: string | null;
  /** Description */
  description?: string | null;
  /** Roles */
  roles?: string[] | null;
}

/** Response */
export interface WeatherForecastResponse {
  /**
   * Date
   * @format date-time
   */
  date?: string;
  /**
   * Temp in ºC
   * @format int32
   */
  temperatureC?: number;
  /**
   * Temp in ºF
   * @format int32
   */
  temperatureF?: number;
  /** Summary */
  summary?: string | null;
}

/** Module settings */
export interface WeatherModuleSettings {
  /**
   * Day count
   * @format int32
   */
  dayCount?: number;
}

/** Save settings request */
export interface WeatherModuleSettingsSaveSettingsRequest {
  /**
   * Module instance id
   * @format int32
   */
  id?: number;
  /** Module settings */
  settings?: WeatherModuleSettings;
}

export interface DashboardGetSecurityListParams {
  /** @format int32 */
  id?: number;
}

export interface DashboardGetModuleInstanceSettingsListParams {
  /** @format int32 */
  id?: number;
}

export interface DbMigrationGetSecurityListParams {
  /** @format int32 */
  id?: number;
}

export interface DbMigrationGetModuleInstanceSettingsListParams {
  /** @format int32 */
  id?: number;
}

export interface FolderGetSecurityListParams {
  /** @format int32 */
  id?: number;
}

export interface FolderGetModuleInstanceSettingsListParams {
  /** @format int32 */
  id?: number;
}

export interface MenuDeleteModuleInstanceDeleteParams {
  /** @format int32 */
  id?: number;
}

export interface UserProfileGetUserPhotoListParams {
  email?: string;
}

export interface UserProfilePostUserPhotoCreatePayload {
  /** @format binary */
  files?: File;
}

export interface WeatherGetListParams {
  /** @format int32 */
  dayCount?: number;
}

export interface WeatherGetSecurityListParams {
  /** @format int32 */
  id?: number;
}

export interface WeatherGetModuleInstanceSettingsListParams {
  /** @format int32 */
  id?: number;
}
