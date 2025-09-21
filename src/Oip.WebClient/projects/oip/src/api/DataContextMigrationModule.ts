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

import {
  ApplyMigrationRequest,
  DbMigrationGetModuleInstanceSettingsParams,
  DbMigrationGetSecurityParams,
  MigrationDto,
  ObjectSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class DataContextMigrationModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationGetModuleRights
   * @summary Gets the module's access rights.
   * @request GET:/api/db-migration/get-module-rights
   * @secure
   */
  dbMigrationGetModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/db-migration/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description The returned list includes: - Migrations that have been applied to the database. - Pending migrations that exist in code but are not applied. - Migrations defined in code regardless of their application status.
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationGetMigrations
   * @summary Retrieves all database migrations and their current state.
   * @request GET:/api/db-migration/get-migrations
   * @secure
   */
  dbMigrationGetMigrations = (params: RequestParams = {}) =>
    this.request<MigrationDto[], any>({
      path: `/api/db-migration/get-migrations`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Uses Entity Framework Core's migration mechanism to bring the database schema up to date with the current codebase. This operation is irreversible and should be performed with caution in production environments.
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationMigrate
   * @summary Applies all pending migrations to the database.
   * @request POST:/api/db-migration/migrate
   * @secure
   */
  dbMigrationMigrate = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/db-migration/migrate`,
      method: "POST",
      secure: true,
      ...params,
    });
  /**
   * @description This method allows applying or reverting to a specific migration by name. Useful for targeted database updates or rolling back schema changes.
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationApplyMigration
   * @summary Applies a specific database migration by name.
   * @request POST:/api/db-migration/apply-migration
   * @secure
   */
  dbMigrationApplyMigration = (
    data: ApplyMigrationRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/db-migration/apply-migration`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationGetSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/db-migration/get-security
   * @secure
   */
  dbMigrationGetSecurity = (
    query: DbMigrationGetSecurityParams,
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/db-migration/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationPutSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/db-migration/put-security
   * @secure
   */
  dbMigrationPutSecurity = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/db-migration/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationGetModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/db-migration/get-module-instance-settings
   * @secure
   */
  dbMigrationGetModuleInstanceSettings = (
    query: DbMigrationGetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/db-migration/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationPutModuleInstanceSettings
   * @request PUT:/api/db-migration/put-module-instance-settings
   * @secure
   */
  dbMigrationPutModuleInstanceSettings = (
    data: ObjectSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/db-migration/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
