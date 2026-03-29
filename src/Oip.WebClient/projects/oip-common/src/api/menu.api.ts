/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  AddModuleInstanceDto,
  ApiExceptionResponse,
  ChangeOrderParams,
  DeleteModuleInstanceParams,
  EditModuleInstanceDto,
  IntKeyValueDto,
  ModuleInstanceDto,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class MenuApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Retrieves the menu available to the current authenticated user.
   *
   * @tags Menu
   * @name get
   * @summary Retrieves the menu available to the current authenticated user.
   * @request GET:/api/menu/get
   * @secure
   * @response `200` `(ModuleInstanceDto)[]` OK
   */
  get = (params: RequestParams = {}) =>
    this.request<ModuleInstanceDto[], any>({
      path: `/api/menu/get`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Retrieves the admin-specific menu.
   *
   * @tags Menu
   * @name getAdminMenu
   * @summary Retrieves the admin-specific menu.
   * @request GET:/api/menu/get-admin-menu
   * @secure
   * @response `200` `(ModuleInstanceDto)[]` OK
   */
  getAdminMenu = (params: RequestParams = {}) =>
    this.request<ModuleInstanceDto[], any>({
      path: `/api/menu/get-admin-menu`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Retrieves all available modules in the system.
   *
   * @tags Menu
   * @name getModules
   * @summary Retrieves all available modules in the system.
   * @request GET:/api/menu/get-modules
   * @secure
   * @response `200` `(IntKeyValueDto)[]` OK
   */
  getModules = (params: RequestParams = {}) =>
    this.request<IntKeyValueDto[], any>({
      path: `/api/menu/get-modules`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Adds a new module instance to the system.
   *
   * @tags Menu
   * @name addModuleInstance
   * @summary Adds a new module instance to the system.
   * @request POST:/api/menu/add-module-instance
   * @secure
   * @response `200` `void` OK
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  addModuleInstance = (
    data: AddModuleInstanceDto,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/menu/add-module-instance`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Edits an existing module instance.
   *
   * @tags Menu
   * @name editModuleInstance
   * @summary Edits an existing module instance.
   * @request POST:/api/menu/edit-module-instance
   * @secure
   * @response `200` `void` OK
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  editModuleInstance = (
    data: EditModuleInstanceDto,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/menu/edit-module-instance`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Deletes a module instance by its identifier.
   *
   * @tags Menu
   * @name deleteModuleInstance
   * @summary Deletes a module instance by its identifier.
   * @request DELETE:/api/menu/delete-module-instance
   * @secure
   * @response `200` `void` OK
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  deleteModuleInstance = (
    query: DeleteModuleInstanceParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/menu/delete-module-instance`,
      method: "DELETE",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * @description Swaps the order positions of two modules in the menu structure.
   *
   * @tags Menu
   * @name changeOrder
   * @summary Swaps the order positions of two modules in the menu structure.
   * @request POST:/api/menu/change-order
   * @secure
   * @response `200` `void` OK
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  changeOrder = (query: ChangeOrderParams, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/menu/change-order`,
      method: "POST",
      query: query,
      secure: true,
      ...params,
    });
}
