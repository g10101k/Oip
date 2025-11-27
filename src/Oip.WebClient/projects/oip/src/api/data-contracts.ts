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

/** Represents the base exception class for Oip applications. */
export interface OipException {
  /** Exception message. */
  message?: string | null;
  /** The HTTP status code associated with the exception. */
  statusCode?: number;
  /** The stack trace for the exception. */
  stackTrace?: string | null;
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
