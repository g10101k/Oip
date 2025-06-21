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
  FolderGetModuleInstanceSettingsParams,
  FolderGetSecurityParams,
  ObjectSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class FolderModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags FolderModule
   * @name folderGetModuleRights
   * @request GET:/api/folder/get-module-rights
   * @secure
   */
  folderGetModuleRights = (params: RequestParams = {}) =>
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
   * @name folderGetSecurity
   * @summary Get security for instance id
   * @request GET:/api/folder/get-security
   * @secure
   */
  folderGetSecurity = (
    query: FolderGetSecurityParams,
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
   * @name folderPutSecurity
   * @summary Update security
   * @request PUT:/api/folder/put-security
   * @secure
   */
  folderPutSecurity = (data: PutSecurityRequest, params: RequestParams = {}) =>
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
   * @name folderGetModuleInstanceSettings
   * @summary Get instance setting
   * @request GET:/api/folder/get-module-instance-settings
   * @secure
   */
  folderGetModuleInstanceSettings = (
    query: FolderGetModuleInstanceSettingsParams,
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
   * @name folderPutModuleInstanceSettings
   * @request PUT:/api/folder/put-module-instance-settings
   * @secure
   */
  folderPutModuleInstanceSettings = (
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
