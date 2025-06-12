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
  FolderGetModuleInstanceSettingsListParams,
  FolderGetSecurityListParams,
  ObjectSaveSettingsRequest,
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
   * @name folderGetModuleRightsList
   * @summary Returns a list of rights (permissions) required to access the folder module.
   * @request GET:/api/folder/get-module-rights
   * @secure
   */
  folderGetModuleRightsList = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/folder/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags FolderModule
   * @name folderGetSecurityList
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/folder/get-security
   * @secure
   */
  folderGetSecurityList = (
    query: FolderGetSecurityListParams,
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/folder/get-security`,
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
   * @name folderPutSecurityUpdate
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/folder/put-security
   * @secure
   */
  folderPutSecurityUpdate = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/folder/put-security`,
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
   * @name folderGetModuleInstanceSettingsList
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/folder/get-module-instance-settings
   * @secure
   */
  folderGetModuleInstanceSettingsList = (
    query: FolderGetModuleInstanceSettingsListParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/folder/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags FolderModule
   * @name folderPutModuleInstanceSettingsUpdate
   * @request PUT:/api/folder/put-module-instance-settings
   * @secure
   */
  folderPutModuleInstanceSettingsUpdate = (
    data: ObjectSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/folder/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
