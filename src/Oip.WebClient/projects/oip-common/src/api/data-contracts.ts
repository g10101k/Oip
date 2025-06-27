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

export interface FolderGetSecurityParams {
  id?: number;
}

export interface MenuDeleteModuleInstanceParams {
  id?: number;
}

export interface SettingsGetModuleInstanceSettingsParams {
  id?: number;
}

export interface SettingsPutModuleInstanceSettingsParams {
  id?: number;
  settings?: string;
}

export interface UserProfileGetUserPhotoParams {
  email?: string;
}

export interface UserProfilePostUserPhotoPayload {
  files?: File;
}
