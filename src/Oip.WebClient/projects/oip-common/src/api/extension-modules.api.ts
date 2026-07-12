/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  DeleteExtensionModuleParams,
  GetExtensionModuleByKeyParams,
  ModuleDto,
  RegisterExtensionModuleRequest,
  UpdateExtensionModuleParams,
  UpdateExtensionModuleRequest,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class ExtensionModulesApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getExtensionModules = (params: RequestParams = {}) =>
    this.request<ModuleDto[], ApiExceptionResponse>({
      path: `/api/extension-modules/get-extension-modules`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getExtensionModuleByKey = (
    { extensionKey, ...query }: GetExtensionModuleByKeyParams,
    params: RequestParams = {},
  ) =>
    this.request<ModuleDto, ApiExceptionResponse>({
      path: `/api/extension-modules/get-extension-module-by-key/${extensionKey}`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  registerExtensionModule = (
    data: RegisterExtensionModuleRequest,
    params: RequestParams = {},
  ) =>
    this.request<ModuleDto, ApiExceptionResponse>({
      path: `/api/extension-modules/register-extension-module`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  updateExtensionModule = (
    { id, ...query }: UpdateExtensionModuleParams,
    data: UpdateExtensionModuleRequest,
    params: RequestParams = {},
  ) =>
    this.request<ModuleDto, ApiExceptionResponse>({
      path: `/api/extension-modules/update-extension-module/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  deleteExtensionModule = (
    { id, ...query }: DeleteExtensionModuleParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/extension-modules/delete-extension-module/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
}
