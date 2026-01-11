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

/** Data Transfer Object for creating a new module instance */
export interface AddModuleInstanceDto {
  /** The identifier of the module to create an instance of */
  moduleId?: number;
  /** The display label for the module instance */
  label?: string | null;
  /** The icon identifier for the module instance (optional) */
  icon?: string | null;
  /** The parent module instance identifier (optional) */
  parentId?: number | null;
  /** Array of role identifiers that can view this module instance (optional) */
  viewRoles?: string[] | null;
}

/** Standardized response format for API exceptions */
export interface ApiExceptionResponse {
  /** User-friendly title of the exception */
  title?: string | null;
  /** Detailed description of the error */
  message?: string | null;
  /** HTTP status code associated with the exception */
  statusCode?: number;
  /** Stack trace information (optional, typically omitted in production) */
  stackTrace?: string | null;
}

/** Request for cryptographic operations with a message to be processed. */
export interface CryptRequest {
  /** The message to be protected or decrypted. */
  message?: string | null;
}

/** Data Transfer Object for editing an existing module instance */
export interface EditModuleInstanceDto {
  /** The identifier of the module instance to edit */
  moduleInstanceId?: number;
  /** The updated display label for the module instance */
  label?: string | null;
  /** The updated icon identifier for the module instance (optional) */
  icon?: string | null;
  /** The updated parent module instance identifier (optional) */
  parentId?: number | null;
  /** Updated array of role identifiers that can view this module instance (optional) */
  viewRoles?: string[] | null;
}

/** Data transfer object representing a module and its loaded status. */
export interface ExistModuleDto {
  /** Gets or sets the unique identifier of the module. */
  moduleId?: number;
  /** Gets or sets the name of the module. */
  name?: string | null;
  /** Gets or sets a value indicating whether the module is currently loaded in the application. */
  currentlyLoaded?: boolean;
}

/** Module settings. */
export interface FolderModuleSettings {
  /** HTML content for the module. */
  html?: string | null;
}

/** Represents a request to save module instance settings. */
export interface FolderModuleSettingsSaveSettingsRequest {
  /** Gets or sets the ID of the module instance. */
  id?: number;
  /** Module settings. */
  settings?: FolderModuleSettings;
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

/** Represents a key-value pair where the key is an integer and the value is a string. */
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
  /** Securities */
  securities?: string[] | null;
}

/** Module security DTO */
export interface ModuleSecurityDto {
  /** Right */
  right: string | null;
  /** Role */
  role: string | null;
}

/** Put security dto */
export interface PutSecurityRequest {
  /** Instance id */
  id?: number;
  /** Securities */
  securities?: SecurityResponse[] | null;
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

export interface FolderModuleGetSecurityParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface FolderModuleGetModuleInstanceSettingsParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface MenuDeleteModuleInstanceParams {
  /** The unique identifier of the module instance to delete. */
  id?: number;
}
