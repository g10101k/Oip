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
  AddModuleInstanceDto,
  EditModuleInstanceDto,
  IntKeyValueDto,
  ModuleInstanceDto,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class MenuDataService<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Menu
   * @name getList
   * @summary Get menu for client app
   * @request GET:/api/menu/get
   * @secure
   */
  getList = (params: RequestParams = {}) =>
    this.request<ModuleInstanceDto[], any>({
      path: `/api/menu/get`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Menu
   * @name getAdminMenuList
   * @summary Get admin menu for client app
   * @request GET:/api/menu/get-admin-menu
   * @secure
   */
  getAdminMenuList = (params: RequestParams = {}) =>
    this.request<ModuleInstanceDto[], any>({
      path: `/api/menu/get-admin-menu`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Menu
   * @name getModulesList
   * @summary Get admin menu for client app
   * @request GET:/api/menu/get-modules
   * @secure
   */
  getModulesList = (params: RequestParams = {}) =>
    this.request<IntKeyValueDto[], any>({
      path: `/api/menu/get-modules`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Menu
   * @name addModuleInstanceCreate
   * @summary Add new module
   * @request POST:/api/menu/add-module-instance
   * @secure
   */
  addModuleInstanceCreate = (
    data: AddModuleInstanceDto,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/menu/add-module-instance`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Menu
   * @name editModuleInstanceCreate
   * @summary Add new module
   * @request POST:/api/menu/edit-module-instance
   * @secure
   */
  editModuleInstanceCreate = (
    data: EditModuleInstanceDto,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/menu/edit-module-instance`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Menu
   * @name deleteModuleInstanceDelete
   * @summary Add new module
   * @request DELETE:/api/menu/delete-module-instance
   * @secure
   */
  deleteModuleInstanceDelete = (
    query?: {
      /** @format int32 */
      id?: number;
    },
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/menu/delete-module-instance`,
      method: "DELETE",
      query: query,
      secure: true,
      ...params,
    });
}
