/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  CustomerModuleSettingsSaveSettingsRequest,
  DeleteParams,
  DemoCustomerTableRowDto,
  DemoCustomerTableRowDtoTablePageResult,
  GetModuleInstanceSettingsParams,
  GetSecurityParams,
  PutSecurityRequest,
  SaveDemoCustomerRequest,
  SecurityResponse,
  TableQueryRequest,
  UpdateParams,
} from "./data-contracts";

@Injectable()
export class CustomerModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Retrieves a filtered page of customers for the customer module table.
   *
   * @tags CustomerModule
   * @name getPage
   * @summary Retrieves a filtered page of customers for the customer module table.
   * @request POST:/api/customer-module/get-page
   * @secure
   * @response `200` `DemoCustomerTableRowDtoTablePageResult` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
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
  /**
   * @description Retrieves available customer categories.
   *
   * @tags CustomerModule
   * @name getCategories
   * @summary Retrieves available customer categories.
   * @request GET:/api/customer-module/get-categories
   * @secure
   * @response `200` `(string)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  getCategories = (params: RequestParams = {}) =>
    this.request<string[], ApiExceptionResponse>({
      path: `/api/customer-module/get-categories`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Retrieves available customer countries.
   *
   * @tags CustomerModule
   * @name getCountries
   * @summary Retrieves available customer countries.
   * @request GET:/api/customer-module/get-countries
   * @secure
   * @response `200` `(string)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  getCountries = (params: RequestParams = {}) =>
    this.request<string[], ApiExceptionResponse>({
      path: `/api/customer-module/get-countries`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Creates a new customer.
   *
   * @tags CustomerModule
   * @name create
   * @summary Creates a new customer.
   * @request POST:/api/customer-module/create
   * @secure
   * @response `200` `DemoCustomerTableRowDto` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
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
  /**
   * @description Updates an existing customer.
   *
   * @tags CustomerModule
   * @name update
   * @summary Updates an existing customer.
   * @request PUT:/api/customer-module/update/{id}
   * @secure
   * @response `200` `DemoCustomerTableRowDto` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `404` `ApiExceptionResponse` Not Found
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
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
  /**
   * @description Deletes a customer.
   *
   * @tags CustomerModule
   * @name delete
   * @summary Deletes a customer.
   * @request DELETE:/api/customer-module/delete/{id}
   * @secure
   * @response `204` `void` No Content
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   * @response `404` `ApiExceptionResponse` Not Found
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  delete = ({ id, ...query }: DeleteParams, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/customer-module/delete/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  /**
   * @description <inheritdoc />
   *
   * @tags CustomerModule
   * @name getModuleRights
   * @summary <inheritdoc />
   * @request GET:/api/customer-module/get-module-rights
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/customer-module/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets the security configuration for the specified module instance ID.
   *
   * @tags CustomerModule
   * @name getSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/customer-module/get-security
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getSecurity = (query: GetSecurityParams, params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/customer-module/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Updates the security configuration for the specified module instance.
   *
   * @tags CustomerModule
   * @name putSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/customer-module/put-security
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  putSecurity = (data: PutSecurityRequest, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/customer-module/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Gets the settings for the specified module instance.
   *
   * @tags CustomerModule
   * @name getModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/customer-module/get-module-instance-settings
   * @secure
   * @response `200` `any` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/customer-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags CustomerModule
   * @name putModuleInstanceSettings
   * @request PUT:/api/customer-module/put-module-instance-settings
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  putModuleInstanceSettings = (
    data: CustomerModuleSettingsSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/customer-module/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
