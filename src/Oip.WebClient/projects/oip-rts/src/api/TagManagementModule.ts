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
  TagEntity,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class TagManagementModuleDataService<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Accepts a tag entity from the request body and stores it in the database. Returns HTTP 200 on success.
   *
   * @tags TagManagementModule
   * @name tagManagementModuleAddTagCreate
   * @summary Adds a new tag.
   * @request POST:/api/tag-management-module/add-tag
   * @secure
   */
  addTagCreate = (data: TagEntity, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/tag-management-module/add-tag`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Returns a list of tags whose names match the provided filter string. This is useful for searching or filtering tags by partial name.
   *
   * @tags TagManagementModule
   * @name tagManagementModuleGetTagsByFilterList
   * @summary Retrieves tags that match a given name filter.
   * @request GET:/api/tag-management-module/get-tags-by-filter
   * @secure
   */
  getTagsByFilterList = (
    query?: {
      /** Name filter to search tags by. */
      filter?: string;
    },
    params: RequestParams = {},
  ) =>
    this.request<TagEntity[], any>({
      path: `/api/tag-management-module/get-tags-by-filter`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description This method defines the access control rules for the Tag Management module. It lists the roles and permissions necessary to interact with the module via the UI or API.
   *
   * @tags TagManagementModule
   * @name tagManagementModuleGetModuleRightsList
   * @summary Returns the security rights required for accessing this module.
   * @request GET:/api/tag-management-module/get-module-rights
   * @secure
   */
  getModuleRightsList = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/tag-management-module/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags TagManagementModule
   * @name tagManagementModuleGetSecurityList
   * @summary Get security for instance id
   * @request GET:/api/tag-management-module/get-security
   * @secure
   */
  getSecurityList = (
    query?: {
      /** @format int32 */
      id?: number;
    },
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/tag-management-module/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags TagManagementModule
   * @name tagManagementModulePutSecurityUpdate
   * @summary Update security
   * @request PUT:/api/tag-management-module/put-security
   * @secure
   */
  putSecurityUpdate = (data: PutSecurityRequest, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/tag-management-module/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags TagManagementModule
   * @name tagManagementModuleGetModuleInstanceSettingsList
   * @summary Get instance setting
   * @request GET:/api/tag-management-module/get-module-instance-settings
   * @secure
   */
  getModuleInstanceSettingsList = (
    query?: {
      /** @format int32 */
      id?: number;
    },
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/tag-management-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags TagManagementModule
   * @name tagManagementModulePutModuleInstanceSettingsUpdate
   * @request PUT:/api/tag-management-module/put-module-instance-settings
   * @secure
   */
  putModuleInstanceSettingsUpdate = (
    data: ObjectSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/tag-management-module/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
