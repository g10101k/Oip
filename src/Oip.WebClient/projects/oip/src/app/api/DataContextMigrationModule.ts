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
  MigrationDto,
  ObjectSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class DataContextMigrationModuleDataService<SecurityDataType = unknown, > extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationGetModuleRightsList
   * @request GET:/api/db-migration/get-module-rights
   * @secure
   */
  dbMigrationGetModuleRightsList = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/db-migration/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationGetMigrationsList
   * @summary Get migration
   * @request GET:/api/db-migration/get-migrations
   * @secure
   */
  dbMigrationGetMigrationsList = (params: RequestParams = {}) =>
    this.request<MigrationDto[], any>({
      path: `/api/db-migration/get-migrations`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationMigrateList
   * @summary Применить миграцию БД
   * @request GET:/api/db-migration/migrate
   * @secure
   */
  dbMigrationMigrateList = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/db-migration/migrate`,
      method: "GET",
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationApplyMigrationCreate
   * @summary Применить миграцию БД
   * @request POST:/api/db-migration/apply-migration
   * @secure
   */
  dbMigrationApplyMigrationCreate = (
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
   * @name dbMigrationGetSecurityList
   * @summary Get security for instance id
   * @request GET:/api/db-migration/get-security
   * @secure
   */
  dbMigrationGetSecurityList = (
    query?: {
      /** @format int32 */
      id?: number;
    },
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
   * @name dbMigrationPutSecurityUpdate
   * @summary Update security
   * @request PUT:/api/db-migration/put-security
   * @secure
   */
  dbMigrationPutSecurityUpdate = (
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
   * @name dbMigrationGetModuleInstanceSettingsList
   * @summary Get instance setting
   * @request GET:/api/db-migration/get-module-instance-settings
   * @secure
   */
  dbMigrationGetModuleInstanceSettingsList = (
    query?: {
      /** @format int32 */
      id?: number;
    },
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
   * @name dbMigrationPutModuleInstanceSettingsUpdate
   * @request PUT:/api/db-migration/put-module-instance-settings
   * @secure
   */
  dbMigrationPutModuleInstanceSettingsUpdate = (
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
