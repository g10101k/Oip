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

/** Defines the data types supported for tags */
export enum TagTypes {
  Float32 = "Float32",
  Float64 = "Float64",
  Int16 = "Int16",
  Int32 = "Int32",
  Digital = "Digital",
  String = "String",
  Blob = "Blob",
}

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

/** Request model for applying a specific migration by name. */
export interface ApplyMigrationRequest {
  /** The name of the migration to apply. */
  name?: string | null;
}

/** Represents the configuration and metadata of a tag. */
export interface CreateTagDto {
  /**
   * Unique identifier of the tag.
   * @format int32
   */
  tagId?: number | null;
  /** Name of the tag. */
  name: string | null;
  /** Defines the data types supported for tags */
  valueType?: TagTypes;
  /**
   * The interface associated with the tag.
   * @format int32
   */
  interface?: number | null;
  /** Description of the point (used as a comment or label). */
  descriptor?: string | null;
  /** Engineering units (e.g., °C, PSI, m³/h). */
  uom?: string | null;
  /** Reference to the source signal or channel tag. */
  instrumentTag?: string | null;
  /** Indicates whether the point is archived. */
  enabled?: boolean;
  /** Indicates whether compression is enabled for this tag. */
  compressing?: boolean;
  /**
   * Minimum time (in milliseconds) between compressed values.
   * Values received within this period are discarded, regardless of their error margin.
   * @format int32
   */
  compressionMinTime?: number | null;
  /**
   * Maximum time (in milliseconds) between compressed values.
   * @format int32
   */
  compressionMaxTime?: number | null;
  /** Associated digital state set name (for digital-type points). */
  digitalSet?: string | null;
  /** Indicates whether values are treated as step (true) or interpolated (false). */
  step?: boolean;
  /**
   * Formula used to calculate the time associated with the tag's value.
   * Default `now()`;
   */
  timeCalculation?: string | null;
  /** Formula used to calculate error values for the tag. */
  errorCalculation?: string | null;
  /** User-defined calculation or formula associated with the tag's value. */
  valueCalculation?: string | null;
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

/** Data transfer object representing a module and its loaded status. */
export interface ExistModuleDto {
  /**
   * Gets or sets the unique identifier of the module.
   * @format int32
   */
  moduleId?: number;
  /** Gets or sets the name of the module. */
  name?: string | null;
  /** Gets or sets a value indicating whether the module is currently loaded in the application. */
  currentlyLoaded?: boolean;
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

export interface InterfaceEntity {
  /** @format int32 */
  id?: number;
  name?: string | null;
  description?: string | null;
  tags?: TagEntity[] | null;
}

/** Data transfer object representing a database migration and its status. */
export interface MigrationDto {
  /** Name of the migration. */
  name?: string | null;
  /** Indicates whether the migration has been applied. */
  applied?: boolean;
  /** Indicates whether the migration is pending. */
  pending?: boolean;
  /** Indicates whether the migration exists in the codebase. */
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
  right: string | null;
  /** Role */
  role: string | null;
}

/** Represents a request to save module instance settings. */
export interface ObjectSaveSettingsRequest {
  /**
   * Gets or sets the ID of the module instance.
   * @format int32
   */
  id?: number;
  /** Gets or sets the settings object to be saved. */
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

/** Represents the configuration and metadata of a tag. */
export interface TagEntity {
  /**
   * Unique identifier of the tag.
   * @format int32
   */
  id?: number;
  /** Name of the tag. */
  name: string | null;
  /** Defines the data types supported for tags */
  valueType?: TagTypes;
  /**
   * The interface associated with the tag.
   * @format int32
   */
  interfaceId?: number | null;
  interface?: InterfaceEntity;
  /** Description of the point (used as a comment or label). */
  descriptor?: string | null;
  /** Engineering units (e.g., °C, PSI, m³/h). */
  uom?: string | null;
  /** Reference to the source signal or channel tag. */
  instrumentTag?: string | null;
  /** Indicates whether the point is archived. */
  enabled?: boolean;
  /** Indicates whether compression is enabled for this tag. */
  compressing?: boolean;
  /**
   * Minimum time (in milliseconds) between compressed values.
   * Values received within this period are discarded, regardless of their error margin.
   * @format int32
   */
  compressionMinTime?: number | null;
  /**
   * Maximum time (in milliseconds) between compressed values.
   * @format int32
   */
  compressionMaxTime?: number | null;
  /** Associated digital state set name (for digital-type points). */
  digitalSet?: string | null;
  /** Indicates whether values are treated as step (true) or interpolated (false). */
  step?: boolean;
  /**
   * Formula used to calculate the time associated with the tag's value.
   * Default `now()`;
   */
  timeCalculation?: string | null;
  /** Formula used to calculate error values for the tag. */
  errorCalculation?: string | null;
  /** User-defined calculation or formula associated with the tag's value. */
  valueCalculation?: string | null;
  /**
   * Date and time when the tag was created.
   * @format date-time
   */
  creationDate?: string;
  /** User or process that created the tag. */
  creator?: string | null;
}

export interface FolderGetSecurityListParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface FolderGetModuleInstanceSettingsListParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface MenuDeleteModuleInstanceDeleteParams {
  /**
   * The unique identifier of the module instance to delete.
   * @format int32
   */
  id?: number;
}

export interface RtdsMetaDataContextMigrationModuleGetSecurityListParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface RtdsMetaDataContextMigrationModuleGetModuleInstanceSettingsListParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface TagManagementGetTagsByFilterListParams {
  /** Name filter to search tags by. */
  filter?: string;
}

export interface TagManagementGetSecurityListParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface TagManagementGetModuleInstanceSettingsListParams {
  /**
   * The ID of the module instance.
   * @format int32
   */
  id?: number;
}

export interface UserProfileGetUserPhotoListParams {
  email?: string;
}

export interface UserProfilePostUserPhotoCreatePayload {
  /** @format binary */
  files?: File;
}
