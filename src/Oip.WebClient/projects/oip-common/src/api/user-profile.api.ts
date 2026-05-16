/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "./http-client";
import {
  ApiExceptionResponse,
  GetUserPhotoParams,
  PostUserPhotoPayload,
  UserSettingsDto,
} from "./user-data-contracts";

@Injectable()
export class UserProfileApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getUserPhoto = (query: GetUserPhotoParams, params: RequestParams = {}) =>
    this.request<File, ApiExceptionResponse>({
      path: `/api/user-profile/get-user-photo`,
      method: "GET",
      query: query,
      secure: true,
      format: "blob",
      ...params,
    });
  postUserPhoto = (data: PostUserPhotoPayload, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/user-profile/post-user-photo`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.FormData,
      ...params,
    });
  getSettings = (params: RequestParams = {}) =>
    this.request<UserSettingsDto, ApiExceptionResponse>({
      path: `/api/user-profile/get-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  setSettings = (data: UserSettingsDto, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/user-profile/set-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
