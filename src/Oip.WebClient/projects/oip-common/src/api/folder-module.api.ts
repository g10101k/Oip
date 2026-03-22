import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  FolderModuleSettingsSaveSettingsRequest,
  GetModuleInstanceSettingsParams,
  GetSecurityParams,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class FolderModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Returns a list of rights (permissions) required to access the folder module.
   *
   * @tags FolderModule
   * @name getModuleRights
   * @summary Returns a list of rights (permissions) required to access the folder module.
   * @request GET:/api/folder-module/get-module-rights
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/folder-module/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets the security configuration for the specified module instance ID.
   *
   * @tags FolderModule
   * @name getSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/folder-module/get-security
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getSecurity = (query: GetSecurityParams, params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/folder-module/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Updates the security configuration for the specified module instance.
   *
   * @tags FolderModule
   * @name putSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/folder-module/put-security
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  putSecurity = (data: PutSecurityRequest, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/folder-module/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Gets the settings for the specified module instance.
   *
   * @tags FolderModule
   * @name getModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/folder-module/get-module-instance-settings
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
      path: `/api/folder-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags FolderModule
   * @name putModuleInstanceSettings
   * @request PUT:/api/folder-module/put-module-instance-settings
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  putModuleInstanceSettings = (
    data: FolderModuleSettingsSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/folder-module/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
