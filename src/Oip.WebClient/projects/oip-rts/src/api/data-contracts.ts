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

/** @format int32 */
export enum TagTypes {
  Value0 = 0,
  Value1 = 1,
  Value2 = 2,
  Value3 = 3,
  Value4 = 4,
  Value5 = 5,
  Value6 = 6,
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

/** Represents the configuration and metadata of a tag. */
export interface TagEntity {
  /**
   * Unique identifier of the tag.
   * @format int32
   */
  tagId?: number;
  /** Name of the tag. */
  name?: string | null;
  valueType?: TagTypes;
  /** Single-letter code indicating the point source (e.g., 'R', 'L'). */
  source?: string | null;
  /** Description of the point (used as a comment or label). */
  descriptor?: string | null;
  /** Engineering units (e.g., °C, PSI, m³/h). */
  engUnits?: string | null;
  /** Reference to the source signal or channel tag. */
  instrumentTag?: string | null;
  /** Indicates whether the point is archived. */
  archiving?: boolean | null;
  /** Indicates whether compression is enabled for this tag. */
  compressing?: boolean | null;
  /**
   * Exception deviation: minimum change required to store a new value.
   * @format double
   */
  excDev?: number | null;
  /**
   * Minimum time (in seconds) between archived values.
   * @format int32
   */
  excMin?: number | null;
  /**
   * Maximum time (in seconds) between archived values.
   * @format int32
   */
  excMax?: number | null;
  /**
   * Compression deviation: minimum change required to pass compression filter.
   * @format double
   */
  compDev?: number | null;
  /**
   * Minimum time (in seconds) between compressed values.
   * @format int32
   */
  compMin?: number | null;
  /**
   * Maximum time (in seconds) between compressed values.
   * @format int32
   */
  compMax?: number | null;
  /**
   * The minimum expected value of the signal.
   * @format int32
   */
  zero?: number | null;
  /**
   * The range between the zero and the maximum value.
   * @format int32
   */
  span?: number | null;
  /**
   * Interface-specific parameter: Location1 (usually the Interface ID).
   * @format int32
   */
  location1?: number | null;
  /**
   * Interface-specific parameter: Location2.
   * @format int32
   */
  location2?: number | null;
  /**
   * Interface-specific parameter: Location3.
   * @format int32
   */
  location3?: number | null;
  /**
   * Interface-specific parameter: Location4.
   * @format int32
   */
  location4?: number | null;
  /**
   * Interface-specific parameter: Location5.
   * @format int32
   */
  location5?: number | null;
  /** Extended description, often used by interfaces. */
  exDesc?: string | null;
  /** Indicates whether the point is being scanned by the interface. */
  scan?: boolean | null;
  /** Associated digital state set name (for digital-type points). */
  digitalSet?: string | null;
  /** Indicates whether values are treated as step (true) or interpolated (false). */
  step?: boolean | null;
  /** Indicates whether this point stores future (forecast) values. */
  future?: boolean | null;
  /**
   * User-defined integer field #1.
   * @format int32
   */
  userInt1?: number | null;
  /**
   * User-defined integer field #2.
   * @format int32
   */
  userInt2?: number | null;
  /**
   * User-defined integer field #3.
   * @format int32
   */
  userInt3?: number | null;
  /**
   * User-defined integer field #4.
   * @format int32
   */
  userInt4?: number | null;
  /**
   * User-defined integer field #5.
   * @format int32
   */
  userInt5?: number | null;
  /**
   * User-defined floating-point field #1.
   * @format double
   */
  userReal1?: number | null;
  /**
   * User-defined floating-point field #2.
   * @format double
   */
  userReal2?: number | null;
  /**
   * User-defined floating-point field #3.
   * @format double
   */
  userReal3?: number | null;
  /**
   * User-defined floating-point field #4.
   * @format double
   */
  userReal4?: number | null;
  /**
   * User-defined floating-point field #5.
   * @format double
   */
  userReal5?: number | null;
  /**
   * Date and time when the tag was created.
   * @format date-time
   */
  creationDate?: string | null;
  /** User or process that created the tag. */
  creator?: string | null;
  /**
   * ClickHouse partitioning clause for time-series storage (e.g., "PARTITION BY toYear(time)").
   * Used to control how data is partitioned when creating the table.
   */
  partition?: string | null;
}
