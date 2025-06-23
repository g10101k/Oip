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
  moduleId?: number;
  label?: string | null;
  icon?: string | null;
  parentId?: number | null;
  viewRoles?: string[] | null;
}

/** Represents a request to apply a specific migration. */
export interface ApplyMigrationRequest {
  /** The name of the migration to apply. */
  name?: string | null;
}

/** Settings */
export interface DashboardSettings {
  /** Just for example */
  nothing?: string | null;
}

/** Save settings request */
export interface DashboardSettingsSaveSettingsRequest {
  /** Module instance id */
  id?: number;
  /** Settings */
  settings?: DashboardSettings;
}

/** DTO for edit module instance */
export interface EditModuleInstanceDto {
  moduleInstanceId?: number;
  label?: string | null;
  icon?: string | null;
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
  /** Log level None = 0, Debug = 1, Warn = 2, Error = 3 */
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
  key?: number;
  value?: string | null;
}

/** DTO represents a migration status. */
export interface MigrationDto {
  /** Name of the migration. */
  name?: string | null;
  /** Indicates whether the migration has been applied to the database. */
  applied?: boolean;
  /** Indicates whether the migration is pending application. */
  pending?: boolean;
  /** Indicates whether the migration exists. */
  exist?: boolean;
}

/** Represents a request to delete a module by its identifier. */
export interface ModuleDeleteRequest {
  /** Gets or sets the unique identifier of the module to be deleted. */
  moduleId?: number;
}

/** It module in app */
export interface ModuleDto {
  /** Id */
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
  /** Unique identifier for the module instance. */
  moduleInstanceId?: number;
  /** Identifier for the module. */
  moduleId?: number;
  /** The label for the module instance. */
  label?: string | null;
  /** Icon associated with the module instance. see https://primeng.org/icons */
  icon?: string | null;
  /** Route link. */
  routerLink?: string[] | null;
  /** URL for the module instance. */
  url?: string | null;
  /** The target. */
  target?: string | null;
  /** Configuration settings for the module instance. */
  settings?: string | null;
  /** Child module instances. */
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
  /** Module instance id */
  id?: number;
  /** Settings */
  settings?: any;
}

/** Put security dto */
export interface PutSecurityRequest {
  /** Instance id */
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
  /** Date */
  date?: Date;
  /** Temp in ºC */
  temperatureC?: number;
  /** Temp in ºF */
  temperatureF?: number;
  /** Summary */
  summary?: string | null;
}

/** Module settings */
export interface WeatherModuleSettings {
  /** Day count */
  dayCount?: number;
}

/** Save settings request */
export interface WeatherModuleSettingsSaveSettingsRequest {
  /** Module instance id */
  id?: number;
  /** Module settings */
  settings?: WeatherModuleSettings;
}

export interface DashboardGetSecurityParams {
  id?: number;
}

export interface DashboardGetModuleInstanceSettingsParams {
  id?: number;
}

export interface DbMigrationGetSecurityParams {
  id?: number;
}

export interface DbMigrationGetModuleInstanceSettingsParams {
  id?: number;
}

export interface FolderGetSecurityParams {
  id?: number;
}

export interface FolderGetModuleInstanceSettingsParams {
  id?: number;
}

export interface MenuDeleteModuleInstanceParams {
  id?: number;
}

export interface UserProfileGetUserPhotoParams {
  email?: string;
}

export interface UserProfilePostUserPhotoPayload {
  files?: File;
}

export interface WeatherForecastModuleGetWeatherForecastParams {
  dayCount?: number;
}

export interface WeatherForecastModuleGetSecurityParams {
  id?: number;
}

export interface WeatherForecastModuleGetModuleInstanceSettingsParams {
  id?: number;
}
