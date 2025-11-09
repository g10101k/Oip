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

import { Injectable } from '@angular/core';
import {
  ApplyMigrationRequest,
  MigrationDto,
  ObjectSaveSettingsRequest,
  PutSecurityRequest,
  RtdsMetaDataContextMigrationModuleGetModuleInstanceSettingsParams,
  RtdsMetaDataContextMigrationModuleGetSecurityParams,
  SecurityResponse
} from './data-contracts';
import { ContentType, HttpClient, RequestParams } from './http-client';

@Injectable()
export class RtdsMetaDataContextMigrationModule<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
  /**
   * @description By default, returns read permission available only for admin role
   *
   * @tags RtdsMetaDataContextMigrationModule
   * @name rtdsMetaDataContextMigrationModuleGetModuleRights
   * @summary Gets the list of access rights for the migration module
   * @request GET:/api/rtds-meta-data-context-migration-module/get-module-rights
   * @secure
   */
  rtdsMetaDataContextMigrationModuleGetModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/rtds-meta-data-context-migration-module/get-module-rights`,
      method: 'GET',
      secure: true,
      format: 'json',
      ...params
    });
  /**
   * @description The returned list includes: - Migrations that have been applied to the database. - Pending migrations that exist in code but are not applied. - Migrations defined in code regardless of their application status.
   *
   * @tags RtdsMetaDataContextMigrationModule
   * @name rtdsMetaDataContextMigrationModuleGetMigrations
   * @summary Retrieves all database migrations and their current state.
   * @request GET:/api/rtds-meta-data-context-migration-module/get-migrations
   * @secure
   */
  rtdsMetaDataContextMigrationModuleGetMigrations = (params: RequestParams = {}) =>
    this.request<MigrationDto[], any>({
      path: `/api/rtds-meta-data-context-migration-module/get-migrations`,
      method: 'GET',
      secure: true,
      format: 'json',
      ...params
    });
  /**
   * @description Uses Entity Framework Core's migration mechanism to bring the database schema up to date with the current codebase. This operation is irreversible and should be performed with caution in production environments.
   *
   * @tags RtdsMetaDataContextMigrationModule
   * @name rtdsMetaDataContextMigrationModuleMigrate
   * @summary Applies all pending migrations to the database.
   * @request POST:/api/rtds-meta-data-context-migration-module/migrate
   * @secure
   */
  rtdsMetaDataContextMigrationModuleMigrate = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/rtds-meta-data-context-migration-module/migrate`,
      method: 'POST',
      secure: true,
      ...params
    });
  /**
   * @description This method allows applying or reverting to a specific migration by name. Useful for targeted database updates or rolling back schema changes.
   *
   * @tags RtdsMetaDataContextMigrationModule
   * @name rtdsMetaDataContextMigrationModuleApplyMigration
   * @summary Applies a specific database migration by name.
   * @request POST:/api/rtds-meta-data-context-migration-module/apply-migration
   * @secure
   */
  rtdsMetaDataContextMigrationModuleApplyMigration = (data: ApplyMigrationRequest, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/rtds-meta-data-context-migration-module/apply-migration`,
      method: 'POST',
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params
    });
  /**
   * No description
   *
   * @tags RtdsMetaDataContextMigrationModule
   * @name rtdsMetaDataContextMigrationModuleGetSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/rtds-meta-data-context-migration-module/get-security
   * @secure
   */
  rtdsMetaDataContextMigrationModuleGetSecurity = (
    query: RtdsMetaDataContextMigrationModuleGetSecurityParams,
    params: RequestParams = {}
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/rtds-meta-data-context-migration-module/get-security`,
      method: 'GET',
      query: query,
      secure: true,
      format: 'json',
      ...params
    });
  /**
   * No description
   *
   * @tags RtdsMetaDataContextMigrationModule
   * @name rtdsMetaDataContextMigrationModulePutSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/rtds-meta-data-context-migration-module/put-security
   * @secure
   */
  rtdsMetaDataContextMigrationModulePutSecurity = (data: PutSecurityRequest, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/rtds-meta-data-context-migration-module/put-security`,
      method: 'PUT',
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params
    });
  /**
   * No description
   *
   * @tags RtdsMetaDataContextMigrationModule
   * @name rtdsMetaDataContextMigrationModuleGetModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/rtds-meta-data-context-migration-module/get-module-instance-settings
   * @secure
   */
  rtdsMetaDataContextMigrationModuleGetModuleInstanceSettings = (
    query: RtdsMetaDataContextMigrationModuleGetModuleInstanceSettingsParams,
    params: RequestParams = {}
  ) =>
    this.request<void, any>({
      path: `/api/rtds-meta-data-context-migration-module/get-module-instance-settings`,
      method: 'GET',
      query: query,
      secure: true,
      ...params
    });
  /**
   * No description
   *
   * @tags RtdsMetaDataContextMigrationModule
   * @name rtdsMetaDataContextMigrationModulePutModuleInstanceSettings
   * @request PUT:/api/rtds-meta-data-context-migration-module/put-module-instance-settings
   * @secure
   */
  rtdsMetaDataContextMigrationModulePutModuleInstanceSettings = (
    data: ObjectSaveSettingsRequest,
    params: RequestParams = {}
  ) =>
    this.request<void, any>({
      path: `/api/rtds-meta-data-context-migration-module/put-module-instance-settings`,
      method: 'PUT',
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params
    });
}
