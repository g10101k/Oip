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

export interface WeatherForecastModuleGetWeatherForecastParams {
  dayCount?: number;
}

export interface WeatherForecastModuleGetSecurityParams {
  id?: number;
}

export interface WeatherForecastModuleGetModuleInstanceSettingsParams {
  id?: number;
}
