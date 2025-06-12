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
   * @description Filters modules based on the roles of the current user and returns only those that are accessible.
   *
   * @tags Menu
   * @name getList
   * @summary Retrieves the menu available to the current authenticated user.
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
   * @description Returns all administrative modules for users with the Admin role.
   *
   * @tags Menu
   * @name getAdminMenuList
   * @summary Retrieves the admin-specific menu.
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
   * @description Useful for module management interfaces or system diagnostics.
   *
   * @tags Menu
   * @name getModulesList
   * @summary Retrieves all available modules in the system.
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
   * @name addModuleInstanceCreate
   * @summary Adds a new module instance to the system.
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
   * @name editModuleInstanceCreate
   * @summary Edits an existing module instance.
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
   * @name deleteModuleInstanceDelete
   * @summary Deletes a module instance by its identifier.
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
