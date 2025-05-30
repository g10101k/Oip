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
  ObjectSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class FolderModuleDataService<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags FolderModule
   * @name folderGetModuleRightsList
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
   * @summary Get security for instance id
   * @request GET:/api/folder/get-security
   * @secure
   */
  folderGetSecurityList = (
    query?: {
      /** @format int32 */
      id?: number;
    },
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
   * @summary Update security
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
   * @summary Get instance setting
   * @request GET:/api/folder/get-module-instance-settings
   * @secure
   */
  folderGetModuleInstanceSettingsList = (
    query?: {
      /** @format int32 */
      id?: number;
    },
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
