/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  CustomerModuleSettings,
  DeleteParams,
  DemoCustomerTableRowDto,
  DemoCustomerTableRowDtoTablePageResult,
  GetModuleInstanceSettingsParams,
  SaveDemoCustomerRequest,
  TableQueryRequest,
  UpdateParams,
} from "./data-contracts";

@Injectable()
export class CustomerModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getPage = (data: TableQueryRequest, params: RequestParams = {}) =>
    this.request<DemoCustomerTableRowDtoTablePageResult, ApiExceptionResponse>({
      path: `/api/customer-module/get-page`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  getCategories = (params: RequestParams = {}) =>
    this.request<string[], ApiExceptionResponse>({
      path: `/api/customer-module/get-categories`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getCountries = (params: RequestParams = {}) =>
    this.request<string[], ApiExceptionResponse>({
      path: `/api/customer-module/get-countries`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  create = (data: SaveDemoCustomerRequest, params: RequestParams = {}) =>
    this.request<DemoCustomerTableRowDto, ApiExceptionResponse>({
      path: `/api/customer-module/create`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  update = (
    { id, ...query }: UpdateParams,
    data: SaveDemoCustomerRequest,
    params: RequestParams = {},
  ) =>
    this.request<DemoCustomerTableRowDto, ApiExceptionResponse>({
      path: `/api/customer-module/update/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  delete = ({ id, ...query }: DeleteParams, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/customer-module/delete/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<CustomerModuleSettings, ApiExceptionResponse>({
      path: `/api/customer-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
