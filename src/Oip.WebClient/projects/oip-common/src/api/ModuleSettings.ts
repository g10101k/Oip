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
  PutSecurityRequest,
  SettingsGetModuleInstanceSettingsParams,
  SettingsPutModuleInstanceSettingsParams,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class ModuleSettings<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags ModuleSettings
   * @name settingsPutSecurity
   * @summary Update security
   * @request PUT:/api/settings/put-security
   * @secure
   */
  settingsPutSecurity = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/settings/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags ModuleSettings
   * @name settingsGetModuleInstanceSettings
   * @summary Get instance setting
   * @request GET:/api/settings/get-module-instance-settings
   * @secure
   */
  settingsGetModuleInstanceSettings = (
    query: SettingsGetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/settings/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags ModuleSettings
   * @name settingsPutModuleInstanceSettings
   * @request PUT:/api/settings/put-module-instance-settings
   * @secure
   */
  settingsPutModuleInstanceSettings = (
    query: SettingsPutModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/settings/put-module-instance-settings`,
      method: "PUT",
      query: query,
      secure: true,
      ...params,
    });
}
