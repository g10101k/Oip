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
   * @name tagManagementAddTagCreate
   * @summary Adds a new tag.
   * @request POST:/api/tag-management/add-tag
   * @secure
   */
  tagManagementAddTagCreate = (data: TagEntity, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/tag-management/add-tag`,
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
   * @name tagManagementGetTagsByFilterList
   * @summary Retrieves tags that match a given name filter.
   * @request GET:/api/tag-management/get-tags-by-filter
   * @secure
   */
  tagManagementGetTagsByFilterList = (
    query?: {
      /** Name filter to search tags by. */
      filter?: string;
    },
    params: RequestParams = {},
  ) =>
    this.request<TagEntity[], any>({
      path: `/api/tag-management/get-tags-by-filter`,
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
   * @name tagManagementGetModuleRightsList
   * @summary Returns the security rights required for accessing this module.
   * @request GET:/api/tag-management/get-module-rights
   * @secure
   */
  tagManagementGetModuleRightsList = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/tag-management/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags TagManagementModule
   * @name tagManagementGetSecurityList
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/tag-management/get-security
   * @secure
   */
  tagManagementGetSecurityList = (
    query?: {
      /**
       * The ID of the module instance.
       * @format int32
       */
      id?: number;
    },
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/tag-management/get-security`,
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
   * @name tagManagementPutSecurityUpdate
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/tag-management/put-security
   * @secure
   */
  tagManagementPutSecurityUpdate = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/tag-management/put-security`,
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
   * @name tagManagementGetModuleInstanceSettingsList
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/tag-management/get-module-instance-settings
   * @secure
   */
  tagManagementGetModuleInstanceSettingsList = (
    query?: {
      /**
       * The ID of the module instance.
       * @format int32
       */
      id?: number;
    },
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/tag-management/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags TagManagementModule
   * @name tagManagementPutModuleInstanceSettingsUpdate
   * @request PUT:/api/tag-management/put-module-instance-settings
   * @secure
   */
  tagManagementPutModuleInstanceSettingsUpdate = (
    data: ObjectSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/tag-management/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
