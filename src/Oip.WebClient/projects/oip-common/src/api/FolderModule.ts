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

import {
  FolderModuleGetModuleInstanceSettingsParams,
  FolderModuleGetSecurityParams,
  FolderModuleSettingsSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class FolderModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description This method defines the security model for the folder module. It currently includes only read access, limited to users with the administrator role.
   *
   * @tags FolderModule
   * @name getModuleRights
   * @summary Returns a list of rights (permissions) required to access the folder module.
   * @request GET:/api/folder-module/get-module-rights
   * @secure
   */
  folderModuleGetModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/folder-module/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags FolderModule
   * @name getSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/folder-module/get-security
   * @secure
   */
  folderModuleGetSecurity = (
    query: FolderModuleGetSecurityParams,
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/folder-module/get-security`,
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
   * @name putSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/folder-module/put-security
   * @secure
   */
  folderModulePutSecurity = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/folder-module/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags FolderModule
   * @name getModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/folder-module/get-module-instance-settings
   * @secure
   */
  folderModuleGetModuleInstanceSettings = (
    query: FolderModuleGetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/folder-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags FolderModule
   * @name putModuleInstanceSettings
   * @request PUT:/api/folder-module/put-module-instance-settings
   * @secure
   */
  folderModulePutModuleInstanceSettings = (
    data: FolderModuleSettingsSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/folder-module/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
