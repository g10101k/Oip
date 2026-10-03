/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  DeleteProviderParams,
  GetLlmProviderModelsRequest,
  LlmProviderDto,
  LlmProviderModuleSettings,
  SaveLlmProviderRequest,
  TestLlmProviderResponse,
  TestProviderParams,
  UpdateProviderParams,
} from "./data-contracts";

@Injectable()
export class LlmProviderModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getProviders = (params: RequestParams = {}) =>
    this.request<LlmProviderDto[], ApiExceptionResponse>({
      path: `/api/llm-provider-module/get-providers`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  createProvider = (data: SaveLlmProviderRequest, params: RequestParams = {}) =>
    this.request<LlmProviderDto, ApiExceptionResponse>({
      path: `/api/llm-provider-module/create-provider`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  updateProvider = (
    { id, ...query }: UpdateProviderParams,
    data: SaveLlmProviderRequest,
    params: RequestParams = {},
  ) =>
    this.request<LlmProviderDto, ApiExceptionResponse>({
      path: `/api/llm-provider-module/update-provider/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  deleteProvider = (
    { id, ...query }: DeleteProviderParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/llm-provider-module/delete-provider/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  testProvider = (
    { id, ...query }: TestProviderParams,
    params: RequestParams = {},
  ) =>
    this.request<TestLlmProviderResponse, ApiExceptionResponse>({
      path: `/api/llm-provider-module/test-provider/${id}`,
      method: "POST",
      secure: true,
      format: "json",
      ...params,
    });
  getProviderModels = (
    data: GetLlmProviderModelsRequest,
    params: RequestParams = {},
  ) =>
    this.request<string[], ApiExceptionResponse>({
      path: `/api/llm-provider-module/get-provider-models`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  getModuleInstanceSettings = (params: RequestParams = {}) =>
    this.request<LlmProviderModuleSettings, ApiExceptionResponse>({
      path: `/api/llm-provider-module/get-module-instance-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
