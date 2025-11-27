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

/** User entity */
export interface UserEntity {
  /** User id */
  userId?: number;
  /** Gets or sets the Keycloak identifier for the user. */
  keycloakId?: string | null;
  /** E-mail */
  email: string | null;
  /** First name */
  firstName?: string | null;
  /** Last name */
  lastName?: string | null;
  /** Indicates whether the user is active */
  isActive?: boolean;
  /** Creation date and time */
  createdAt?: Date;
  /** Last update date and time */
  updatedAt?: Date;
  /** Last synchronization date and time */
  lastSyncedAt?: Date;
  /** User photo */
  photo?: string | null;
}

export interface UsersGetAllUsersParams {
  /** Number of records to skip */
  skip?: number;
  /** Number of records to take */
  take?: number;
}

export interface UsersGetUserParams {
  /** User ID */
  id?: number;
}

export interface UsersGetUserByKeycloakIdParams {
  /** Keycloak user ID */
  keycloakId?: string;
}

export interface UsersSearchUserParams {
  /** Search term */
  term?: string;
}
