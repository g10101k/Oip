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
  get = (params: RequestParams = {}) =>
    this.request<ModuleInstanceDto[], any>({
      path: `/api/menu/get`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getAdminMenu = (params: RequestParams = {}) =>
    this.request<ModuleInstanceDto[], any>({
      path: `/api/menu/get-admin-menu`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getModules = (params: RequestParams = {}) =>
    this.request<IntKeyValueDto[], any>({
      path: `/api/menu/get-modules`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
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
  changeOrder = (query: ChangeOrderParams, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/menu/change-order`,
      method: "POST",
      query: query,
      secure: true,
      ...params,
    });
}
