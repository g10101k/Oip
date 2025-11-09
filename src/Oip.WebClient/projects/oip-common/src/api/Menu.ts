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

import { Injectable } from '@angular/core';
import {
  AddModuleInstanceDto,
  EditModuleInstanceDto,
  IntKeyValueDto,
  MenuDeleteModuleInstanceParams,
  ModuleInstanceDto
} from './data-contracts';
import { ContentType, HttpClient, RequestParams } from './http-client';

@Injectable()
export class Menu<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
  /**
   * @description Retrieves the menu available to the current authenticated user.
   *
   * @tags Menu
   * @name menuGet
   * @request GET:/api/menu/get
   * @secure
   */
  menuGet = (params: RequestParams = {}) =>
    this.request<ModuleInstanceDto[], any>({
      path: `/api/menu/get`,
      method: 'GET',
      secure: true,
      format: 'json',
      ...params
    });
  /**
   * @description Retrieves the admin-specific menu.
   *
   * @tags Menu
   * @name menuGetAdminMenu
   * @request GET:/api/menu/get-admin-menu
   * @secure
   */
  menuGetAdminMenu = (params: RequestParams = {}) =>
    this.request<ModuleInstanceDto[], any>({
      path: `/api/menu/get-admin-menu`,
      method: 'GET',
      secure: true,
      format: 'json',
      ...params
    });
  /**
   * @description Retrieves all available modules in the system.
   *
   * @tags Menu
   * @name menuGetModules
   * @request GET:/api/menu/get-modules
   * @secure
   */
  menuGetModules = (params: RequestParams = {}) =>
    this.request<IntKeyValueDto[], any>({
      path: `/api/menu/get-modules`,
      method: 'GET',
      secure: true,
      format: 'json',
      ...params
    });
  /**
   * @description Adds a new module instance to the system.
   *
   * @tags Menu
   * @name menuAddModuleInstance
   * @request POST:/api/menu/add-module-instance
   * @secure
   */
  menuAddModuleInstance = (data: AddModuleInstanceDto, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/menu/add-module-instance`,
      method: 'POST',
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params
    });
  /**
   * @description Edits an existing module instance.
   *
   * @tags Menu
   * @name menuEditModuleInstance
   * @request POST:/api/menu/edit-module-instance
   * @secure
   */
  menuEditModuleInstance = (data: EditModuleInstanceDto, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/menu/edit-module-instance`,
      method: 'POST',
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params
    });
  /**
   * @description Deletes a module instance by its identifier.
   *
   * @tags Menu
   * @name menuDeleteModuleInstance
   * @request DELETE:/api/menu/delete-module-instance
   * @secure
   */
  menuDeleteModuleInstance = (query: MenuDeleteModuleInstanceParams, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/menu/delete-module-instance`,
      method: 'DELETE',
      query: query,
      secure: true,
      ...params
    });
}
