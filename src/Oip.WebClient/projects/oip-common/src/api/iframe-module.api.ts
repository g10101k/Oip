/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  GetModuleInstanceSettingsParams2,
  GetSecurityParams2,
  IframeModuleSettingsSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class IframeModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags IframeModule
   * @name getModuleRights
   * @request GET:/api/iframe-module/get-module-rights
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/iframe-module/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets the security configuration for the specified module instance ID.
   *
   * @tags IframeModule
   * @name getSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/iframe-module/get-security
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getSecurity = (query: GetSecurityParams2, params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/iframe-module/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Updates the security configuration for the specified module instance.
   *
   * @tags IframeModule
   * @name putSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/iframe-module/put-security
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  putSecurity = (data: PutSecurityRequest, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/iframe-module/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Gets the settings for the specified module instance.
   *
   * @tags IframeModule
   * @name getModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/iframe-module/get-module-instance-settings
   * @secure
   * @response `200` `any` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams2,
    params: RequestParams = {},
  ) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/iframe-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags IframeModule
   * @name putModuleInstanceSettings
   * @request PUT:/api/iframe-module/put-module-instance-settings
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  putModuleInstanceSettings = (
    data: IframeModuleSettingsSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/iframe-module/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
