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

import { Injectable } from "@angular/core";
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

@Injectable()
export class DataContextMigrationModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationGetModuleRights
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
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationGetMigrations
   * @summary Get migration
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
   * No description
   *
   * @tags DataContextMigrationModule
   * @name dbMigrationMigrate
   * @summary Применить миграцию БД
   * @request GET:/api/db-migration/migrate
   * @secure
   */
  dbMigrationMigrate = (params: RequestParams = {}) =>
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
   * @name dbMigrationApplyMigration
   * @summary Применить миграцию БД
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
   * @summary Get security for instance id
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
   * @summary Update security
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
   * @summary Get instance setting
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
