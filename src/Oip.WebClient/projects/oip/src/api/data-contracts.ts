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

/** Request model for applying a specific migration by name. */
export interface ApplyMigrationRequest {
  /** The name of the migration to apply. */
  name?: string | null;
}

/** Settings */
export interface DashboardSettings {
  /** Just, for example */
  nothing?: string | null;
}

/** Represents a request to save module instance settings. */
export interface DashboardSettingsSaveSettingsRequest {
  /** Gets or sets the ID of the module instance. */
  id?: number;
  /** Settings */
  settings?: DashboardSettings;
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

/** Represents a request to save module instance settings. */
export interface WeatherModuleSettingsSaveSettingsRequest {
  /** Gets or sets the ID of the module instance. */
  id?: number;
  /** Module settings */
  settings?: WeatherModuleSettings;
}

export interface DashboardGetSecurityParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface DashboardGetModuleInstanceSettingsParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface DbMigrationGetSecurityParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface DbMigrationGetModuleInstanceSettingsParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface WeatherForecastModuleGetWeatherForecastParams {
  dayCount?: number;
}

export interface WeatherForecastModuleGetSecurityParams {
  /** The ID of the module instance. */
  id?: number;
}

export interface WeatherForecastModuleGetModuleInstanceSettingsParams {
  /** The ID of the module instance. */
  id?: number;
}
