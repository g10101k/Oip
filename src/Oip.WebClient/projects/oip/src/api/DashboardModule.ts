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
  DashboardGetModuleInstanceSettingsParams,
  DashboardGetSecurityParams,
  DashboardSettingsSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable({ providedIn: "root" })
export class DashboardModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags DashboardModule
   * @name dashboardGetModuleRights
   * @request GET:/api/dashboard/get-module-rights
   * @secure
   */
  dashboardGetModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/dashboard/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DashboardModule
   * @name dashboardGetSecurity
   * @summary Get security for instance id
   * @request GET:/api/dashboard/get-security
   * @secure
   */
  dashboardGetSecurity = (
    query: DashboardGetSecurityParams,
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/dashboard/get-security`,
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
   * @name dashboardPutSecurity
   * @summary Update security
   * @request PUT:/api/dashboard/put-security
   * @secure
   */
  dashboardPutSecurity = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/dashboard/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags DashboardModule
   * @name dashboardGetModuleInstanceSettings
   * @summary Get instance setting
   * @request GET:/api/dashboard/get-module-instance-settings
   * @secure
   */
  dashboardGetModuleInstanceSettings = (
    query: DashboardGetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/dashboard/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
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
    this.request<void, any>({
      path: `/api/dashboard/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
