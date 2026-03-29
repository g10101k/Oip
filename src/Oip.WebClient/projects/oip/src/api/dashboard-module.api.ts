/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  DashboardGetModuleInstanceSettingsParams,
  DashboardGetSecurityParams,
  DashboardSettingsSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";

@Injectable()
export class DashboardModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Gets the security configuration for the specified module instance ID.
   *
   * @tags DashboardModule
   * @name dashboardGetSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/dashboard/get-security
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  dashboardGetSecurity = (
    query: DashboardGetSecurityParams,
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/dashboard/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Updates the security configuration for the specified module instance.
   *
   * @tags DashboardModule
   * @name dashboardPutSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/dashboard/put-security
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  dashboardPutSecurity = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/dashboard/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Gets the settings for the specified module instance.
   *
   * @tags DashboardModule
   * @name dashboardGetModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/dashboard/get-module-instance-settings
   * @secure
   * @response `200` `any` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  dashboardGetModuleInstanceSettings = (
    query: DashboardGetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/dashboard/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DashboardModule
   * @name dashboardPutModuleInstanceSettings
   * @request PUT:/api/dashboard/put-module-instance-settings
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  dashboardPutModuleInstanceSettings = (
    data: DashboardSettingsSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/dashboard/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Gets the list of security rights supported by the module.
   *
   * @tags DashboardModule
   * @name dashboardGetModuleRights
   * @summary Gets the list of security rights supported by the module.
   * @request GET:/api/dashboard/get-module-rights
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  dashboardGetModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/dashboard/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
