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

import { DashboardSettingsSaveSettingsRequest, PutSecurityRequest, SecurityResponse, } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class DashboardModuleDataService<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags DashboardModule
   * @name dashboardGetModuleRightsList
   * @request GET:/api/dashboard/get-module-rights
   * @secure
   */
  dashboardGetModuleRightsList = (params: RequestParams = {}) =>
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
   * @name dashboardGetSecurityList
   * @summary Get security for instance id
   * @request GET:/api/dashboard/get-security
   * @secure
   */
  dashboardGetSecurityList = (
    query?: {
      /** @format int32 */
      id?: number;
    },
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
   * @name dashboardPutSecurityUpdate
   * @summary Update security
   * @request PUT:/api/dashboard/put-security
   * @secure
   */
  dashboardPutSecurityUpdate = (
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
   * @name dashboardGetModuleInstanceSettingsList
   * @summary Get instance setting
   * @request GET:/api/dashboard/get-module-instance-settings
   * @secure
   */
  dashboardGetModuleInstanceSettingsList = (
    query?: {
      /** @format int32 */
      id?: number;
    },
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
   * @name dashboardPutModuleInstanceSettingsUpdate
   * @request PUT:/api/dashboard/put-module-instance-settings
   * @secure
   */
  dashboardPutModuleInstanceSettingsUpdate = (
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
