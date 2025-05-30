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

import { ModuleDto } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class ModuleDataService<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Module
   * @name moduleGetAllList
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
   * @name moduleInsertCreate
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
}
