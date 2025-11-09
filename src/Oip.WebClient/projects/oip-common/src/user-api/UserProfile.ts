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
  UserProfileGetUserPhotoParams,
  UserProfilePostUserPhotoPayload,
  UserSettingsDto,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class UserProfile<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Gets user photo by email address
   *
   * @tags UserProfile
   * @name userProfileGetUserPhoto
   * @request GET:/api/user-profile/get-user-photo
   * @secure
   */
  userProfileGetUserPhoto = (
    query: UserProfileGetUserPhotoParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/user-profile/get-user-photo`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * @description Uploads user photo
   *
   * @tags UserProfile
   * @name userProfilePostUserPhoto
   * @request POST:/api/user-profile/post-user-photo
   * @secure
   */
  userProfilePostUserPhoto = (
    data: UserProfilePostUserPhotoPayload,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/user-profile/post-user-photo`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.FormData,
      ...params,
    });
  /**
   * @description Get user setting by e-mail
   *
   * @tags UserProfile
   * @name userProfileGetSettings
   * @request GET:/api/user-profile/get-settings
   * @secure
   */
  userProfileGetSettings = (params: RequestParams = {}) =>
    this.request<UserSettingsDto, any>({
      path: `/api/user-profile/get-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Update User settings
   *
   * @tags UserProfile
   * @name userProfileSetSettings
   * @request PUT:/api/user-profile/set-settings
   * @secure
   */
  userProfileSetSettings = (
    data: UserSettingsDto,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/user-profile/set-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
