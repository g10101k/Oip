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

import { UserProfileGetUserPhotoListParams, UserProfilePostUserPhotoCreatePayload, } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class UserProfile<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags UserProfile
   * @name userProfileGetUserPhotoList
   * @summary Get all roles
   * @request GET:/api/user-profile/get-user-photo
   * @secure
   */
  userProfileGetUserPhotoList = (
    query: UserProfileGetUserPhotoListParams,
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
   * @name userProfilePostUserPhotoCreate
   * @summary Get all roles
   * @request POST:/api/user-profile/post-user-photo
   * @secure
   */
  userProfilePostUserPhotoCreate = (
    data: UserProfilePostUserPhotoCreatePayload,
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
