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
  ExistModuleDto,
  ModuleDeleteRequest,
  ModuleDto,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class Module<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Only accessible to users with the Admin role.
   *
   * @tags Module
   * @name moduleGetAll
   * @summary Retrieves all modules stored in the system.
   * @request GET:/api/module/get-all
   * @secure
   */
  moduleGetAll = (params: RequestParams = {}) =>
    this.request<ModuleDto[], any>({
      path: `/api/module/get-all`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Module
   * @name moduleInsert
   * @summary Inserts a new module into the system.
   * @request POST:/api/module/insert
   * @secure
   */
  moduleInsert = (data: ModuleDto, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/module/insert`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Module
   * @name moduleDelete
   * @summary Deletes a module by its identifier.
   * @request DELETE:/api/module/delete
   * @secure
   */
  moduleDelete = (data: ModuleDeleteRequest, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/module/delete`,
      method: "DELETE",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Compares all modules in the database with loaded modules in the application context. This information can be used for diagnostics and monitoring of active modules.
   *
   * @tags Module
   * @name moduleGetModulesWithLoadStatus
   * @summary Returns all registered modules and indicates whether each one is currently loaded into the application.
   * @request GET:/api/module/get-modules-with-load-status
   * @secure
   */
  moduleGetModulesWithLoadStatus = (params: RequestParams = {}) =>
    this.request<ExistModuleDto[], any>({
      path: `/api/module/get-modules-with-load-status`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
