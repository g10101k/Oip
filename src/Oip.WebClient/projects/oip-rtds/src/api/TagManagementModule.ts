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
  CreateTagDto,
  ObjectSaveSettingsRequest,
  PutSecurityRequest,
  SecurityResponse,
  TagDto,
  TagManagementGetModuleInstanceSettingsParams,
  TagManagementGetSecurityParams,
  TagManagementGetTagsByFilterParams,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class TagManagementModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Accepts a tag entity from the request body and stores it in the database. Returns HTTP 200 on success.
   *
   * @tags TagManagementModule
   * @name tagManagementAddTag
   * @summary Adds a new tag.
   * @request POST:/api/tag-management/add-tag
   * @secure
   */
  tagManagementAddTag = (data: CreateTagDto, params: RequestParams = {}) =>
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
   * @name tagManagementGetTagsByFilter
   * @summary Retrieves tags that match a given name filter.
   * @request GET:/api/tag-management/get-tags-by-filter
   * @secure
   */
  tagManagementGetTagsByFilter = (
    query: TagManagementGetTagsByFilterParams,
    params: RequestParams = {},
  ) =>
    this.request<TagDto[], any>({
      path: `/api/tag-management/get-tags-by-filter`,
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
   * @name tagManagementEditTag
   * @summary Edits an existing tag.
   * @request POST:/api/tag-management/edit-tag
   * @secure
   */
  tagManagementEditTag = (data: CreateTagDto, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/tag-management/edit-tag`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description This method defines the access control rules for the Tag Management module. It lists the roles and permissions necessary to interact with the module via the UI or API.
   *
   * @tags TagManagementModule
   * @name tagManagementGetModuleRights
   * @summary Returns the security rights required for accessing this module.
   * @request GET:/api/tag-management/get-module-rights
   * @secure
   */
  tagManagementGetModuleRights = (params: RequestParams = {}) =>
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
   * @name tagManagementGetSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/tag-management/get-security
   * @secure
   */
  tagManagementGetSecurity = (
    query: TagManagementGetSecurityParams,
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
   * @name tagManagementPutSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/tag-management/put-security
   * @secure
   */
  tagManagementPutSecurity = (
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
   * @name tagManagementGetModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/tag-management/get-module-instance-settings
   * @secure
   */
  tagManagementGetModuleInstanceSettings = (
    query: TagManagementGetModuleInstanceSettingsParams,
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
   * @name tagManagementPutModuleInstanceSettings
   * @request PUT:/api/tag-management/put-module-instance-settings
   * @secure
   */
  tagManagementPutModuleInstanceSettings = (
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
