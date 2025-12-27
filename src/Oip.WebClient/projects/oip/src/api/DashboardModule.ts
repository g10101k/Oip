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

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  DashboardGetModuleInstanceSettingsParams,
  DashboardGetSecurityParams,
  DashboardSettingsSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class DashboardModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Gets the security configuration for the specified module instance ID.
   *
   * @tags DashboardModule
   * @name dashboardGetSecurity
   * @request GET:/api/dashboard/get-security
   * @secure
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
   * @request PUT:/api/dashboard/put-security
   * @secure
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
   * @request GET:/api/dashboard/get-module-instance-settings
   * @secure
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
   * @request GET:/api/dashboard/get-module-rights
   * @secure
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
