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
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable({ providedIn: "root" })
export class UserProfile<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags UserProfile
   * @name userProfileGetUserPhoto
   * @summary Get all roles
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
   * No description
   *
   * @tags UserProfile
   * @name userProfilePostUserPhoto
   * @summary Get all roles
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
}
