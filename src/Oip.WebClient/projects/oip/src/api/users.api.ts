/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  GetAllUsersParams,
  GetUserByKeycloakIdParams,
  GetUserParams,
  SearchUserParams,
  SyncUserRequest,
  UserEntity,
} from "./data-contracts";

@Injectable()
export class UsersApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getAllUsers = (query: GetAllUsersParams, params: RequestParams = {}) =>
    this.request<UserEntity[], any>({
      path: `/api/users/get-all-users`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  getUser = (query: GetUserParams, params: RequestParams = {}) =>
    this.request<UserEntity, any>({
      path: `/api/users/get-user`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  getUserByKeycloakId = (
    query: GetUserByKeycloakIdParams,
    params: RequestParams = {},
  ) =>
    this.request<UserEntity, any>({
      path: `/api/users/get-user-by-keycloak-id`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  searchUser = (query: SearchUserParams, params: RequestParams = {}) =>
    this.request<UserEntity[], any>({
      path: `/api/users/search-user`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  syncUser = (data: SyncUserRequest, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/users/sync-user`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  syncAllUsers = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/users/sync-all-users`,
      method: "POST",
      secure: true,
      ...params,
    });
}
