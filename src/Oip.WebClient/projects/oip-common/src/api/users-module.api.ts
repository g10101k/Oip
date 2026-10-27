/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "./http-client";
import {
  ApiExceptionResponse,
  GetUsersByFilterRequest,
  UsersPageDto,
  UsersSummaryDto,
  UsersSyncStatus,
} from "./users-data-contracts";

@Injectable()
export class UsersModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getUsersByFilter = (
    data: GetUsersByFilterRequest,
    params: RequestParams = {},
  ) =>
    this.request<UsersPageDto, ApiExceptionResponse>({
      path: `/api/users-module/get-users-by-filter`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  getUsersSummary = (params: RequestParams = {}) =>
    this.request<UsersSummaryDto, ApiExceptionResponse>({
      path: `/api/users-module/get-users-summary`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  syncAllUsers = (params: RequestParams = {}) =>
    this.request<UsersSyncStatus, ApiExceptionResponse>({
      path: `/api/users-module/sync-all-users`,
      method: "POST",
      secure: true,
      format: "json",
      ...params,
    });
  getSyncStatus = (params: RequestParams = {}) =>
    this.request<UsersSyncStatus, ApiExceptionResponse>({
      path: `/api/users-module/get-sync-status`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getModuleInstanceSettings = (params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/users-module/get-module-instance-settings`,
      method: "GET",
      secure: true,
      ...params,
    });
}
