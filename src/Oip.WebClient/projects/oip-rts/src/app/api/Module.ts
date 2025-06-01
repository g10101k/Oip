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

import { ModuleDeleteRequest, ModuleDto } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class ModuleDataService<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Module
   * @name getAllList
   * @summary Get all modules
   * @request GET:/api/module/get-all
   * @secure
   */
  getAllList = (params: RequestParams = {}) =>
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
   * @name insertCreate
   * @summary Insert
   * @request POST:/api/module/insert
   * @secure
   */
  insertCreate = (data: ModuleDto, params: RequestParams = {}) =>
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
   * @name deleteDelete
   * @summary delete
   * @request DELETE:/api/module/delete
   * @secure
   */
  deleteDelete = (data: ModuleDeleteRequest, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/module/delete`,
      method: "DELETE",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description This endpoint is restricted to users with administrative privileges. It aggregates module data from the database and compares it against the currently loaded modules in the application context, returning a combined view with load status flags.
   *
   * @tags Module
   * @name getModulesWithLoadStatusList
   * @summary Returns a list of all registered modules and indicates whether each one is currently loaded into the application.
   * @request GET:/api/module/get-modules-with-load-status
   * @secure
   */
  getModulesWithLoadStatusList = (params: RequestParams = {}) =>
    this.request<ModuleDto[], any>({
      path: `/api/module/get-modules-with-load-status`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
