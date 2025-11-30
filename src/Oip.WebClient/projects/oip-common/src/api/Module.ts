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
  ApiExceptionResponse,
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
   * @description Retrieves all modules stored in the system.
   *
   * @tags Module
   * @name moduleGetAll
   * @request GET:/api/module/get-all
   * @secure
   */
  moduleGetAll = (params: RequestParams = {}) =>
    this.request<ModuleDto[], ApiExceptionResponse>({
      path: `/api/module/get-all`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Inserts a new module into the system.
   *
   * @tags Module
   * @name moduleInsert
   * @request POST:/api/module/insert
   * @secure
   */
  moduleInsert = (data: ModuleDto, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/module/insert`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Deletes a module by its identifier.
   *
   * @tags Module
   * @name moduleDelete
   * @request DELETE:/api/module/delete
   * @secure
   */
  moduleDelete = (data: ModuleDeleteRequest, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/module/delete`,
      method: "DELETE",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Returns all registered modules and indicates whether each one is currently loaded into the application.
   *
   * @tags Module
   * @name moduleGetModulesWithLoadStatus
   * @request GET:/api/module/get-modules-with-load-status
   * @secure
   */
  moduleGetModulesWithLoadStatus = (params: RequestParams = {}) =>
    this.request<ExistModuleDto[], ApiExceptionResponse>({
      path: `/api/module/get-modules-with-load-status`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
