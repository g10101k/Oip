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
  MenuDeleteModuleInstanceDeleteParams,
  ModuleInstanceDto,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class Menu<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Menu
   * @name menuGetList
   * @summary Get menu for client app
   * @request GET:/api/menu/get
   * @secure
   */
  menuGetList = (params: RequestParams = {}) =>
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
   * @name menuGetAdminMenuList
   * @summary Get admin menu for client app
   * @request GET:/api/menu/get-admin-menu
   * @secure
   */
  menuGetAdminMenuList = (params: RequestParams = {}) =>
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
   * @name menuGetModulesList
   * @summary Get admin menu for client app
   * @request GET:/api/menu/get-modules
   * @secure
   */
  menuGetModulesList = (params: RequestParams = {}) =>
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
   * @name menuAddModuleInstanceCreate
   * @summary Add new module
   * @request POST:/api/menu/add-module-instance
   * @secure
   */
  menuAddModuleInstanceCreate = (
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
   * @name menuEditModuleInstanceCreate
   * @summary Add new module
   * @request POST:/api/menu/edit-module-instance
   * @secure
   */
  menuEditModuleInstanceCreate = (
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
   * @name menuDeleteModuleInstanceDelete
   * @summary Add new module
   * @request DELETE:/api/menu/delete-module-instance
   * @secure
   */
  menuDeleteModuleInstanceDelete = (
    query: MenuDeleteModuleInstanceDeleteParams,
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
