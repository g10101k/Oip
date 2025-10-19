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
  Float32 = 'Float32',
  Float64 = 'Float64',
  Int16 = 'Int16',
  Int32 = 'Int32',
  Digital = 'Digital',
  String = 'String',
  Blob = 'Blob'
} /** Request model for applying a specific migration by name. */
export interface ApplyMigrationRequest {
  /** The name of the migration to apply. */
  name?: string | null;
}

/** Represents the configuration and metadata of a tag. */
export interface CreateTagDto {
  /** Unique identifier of the tag. */
  id?: number | null;
  /** Name of the tag. */
  name: string | null;
  /** Defines the data types supported for tags */
  valueType?: TagTypes;
  /** The interface associated with the tag. */
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
  /** Minimum time (in milliseconds) between compressed values.
Values received within this period are discarded, regardless of their error margin. */
  compressionMinTime?: number | null;
  /** Maximum time (in milliseconds) between compressed values. */
  compressionMaxTime?: number | null;
  /** Associated digital state set name (for digital-type points). */
  digitalSet?: string | null;
  /** Indicates whether values are treated as step (true) or interpolated (false). */
  step?: boolean;
  /** Formula used to calculate the time associated with the tag's value.
Default `now()`; */
  timeCalculation?: string | null;
  /** Formula used to calculate error values for the tag. */
  errorCalculation?: string | null;
  /** User-defined calculation or formula associated with the tag's value. */
  valueCalculation?: string | null;
}

export interface InterfaceEntity {
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

/** Represents a request to save module instance settings. */
export interface ObjectSaveSettingsRequest {
  /** Gets or sets the ID of the module instance. */
  id?: number;
  /** Gets or sets the settings object to be saved. */
  settings?: any;
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

/** Represents the configuration and metadata of a tag. */
export interface TagEntity {
  /** Unique identifier of the tag. */
  id?: number;
  /** Name of the tag. */
  name: string | null;
  /** Defines the data types supported for tags */
  valueType?: TagTypes;
  /** The interface associated with the tag. */
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
  /** Minimum time (in milliseconds) between compressed values.
Values received within this period are discarded, regardless of their error margin. */
  compressionMinTime?: number | null;
  /** Maximum time (in milliseconds) between compressed values. */
  compressionMaxTime?: number | null;
  /** Associated digital state set name (for digital-type points). */
  digitalSet?: string | null;
  /** Indicates whether values are treated as step (true) or interpolated (false). */
  step?: boolean;
  /** Formula used to calculate the time associated with the tag's value.
Default `now()`; */
  timeCalculation?: string | null;
  /** Formula used to calculate error values for the tag. */
  errorCalculation?: string | null;
  /** User-defined calculation or formula associated with the tag's value. */
  valueCalculation?: string | null;
  /** Date and time when the tag was created. */
  creationDate?: Date;
  /** User or process that created the tag. */
  creator?: string | null;
}

export interface RtdsMetaDataContextMigrationModuleGetSecurityParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface RtdsMetaDataContextMigrationModuleGetModuleInstanceSettingsParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface TagManagementGetTagsByFilterParams {
  /** Name filter to search tags by. */
  filter?: string;
}

export interface TagManagementGetSecurityParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface TagManagementGetModuleInstanceSettingsParams {
  /** The ID of the module instance. */
  id?: number;
}
