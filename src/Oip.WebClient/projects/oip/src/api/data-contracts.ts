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

/** Represents a request to synchronize a user from Keycloak */
export interface SyncUserRequest {
  /** The unique identifier of the user in Keycloak */
  keycloakUserId?: string | null;
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
