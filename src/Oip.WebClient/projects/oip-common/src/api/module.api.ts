/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  ExistModuleDto,
  ModuleDeleteRequest,
  ModuleDto,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class ModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getAll = (params: RequestParams = {}) =>
    this.request<ModuleDto[], ApiExceptionResponse>({
      path: `/api/module/get-all`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  insert = (data: ModuleDto, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/module/insert`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  delete = (data: ModuleDeleteRequest, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/module/delete`,
      method: "DELETE",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  getModulesWithLoadStatus = (params: RequestParams = {}) =>
    this.request<ExistModuleDto[], ApiExceptionResponse>({
      path: `/api/module/get-modules-with-load-status`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
