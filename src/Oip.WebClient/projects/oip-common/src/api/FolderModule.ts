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
  FolderModuleGetModuleInstanceSettingsParams,
  FolderModuleGetSecurityParams,
  FolderModuleSettingsSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class FolderModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Returns a list of rights (permissions) required to access the folder module.
   *
   * @tags FolderModule
   * @name folderModuleGetModuleRights
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
   * @description Gets the security configuration for the specified module instance ID.
   *
   * @tags FolderModule
   * @name folderModuleGetSecurity
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
   * @description Updates the security configuration for the specified module instance.
   *
   * @tags FolderModule
   * @name folderModulePutSecurity
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
   * @description Gets the settings for the specified module instance.
   *
   * @tags FolderModule
   * @name folderModuleGetModuleInstanceSettings
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
   * @name folderModulePutModuleInstanceSettings
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
