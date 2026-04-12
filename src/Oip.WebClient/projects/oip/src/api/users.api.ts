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
  /**
   * @description Gets a list of users with pagination
   *
   * @tags Users
   * @name getAllUsers
   * @summary Gets a list of users with pagination
   * @request GET:/api/users/get-all-users
   * @secure
   * @response `200` `(UserEntity)[]` OK
   */
  getAllUsers = (query: GetAllUsersParams, params: RequestParams = {}) =>
    this.request<UserEntity[], any>({
      path: `/api/users/get-all-users`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets a user by ID
   *
   * @tags Users
   * @name getUser
   * @summary Gets a user by ID
   * @request GET:/api/users/get-user
   * @secure
   * @response `200` `UserEntity` OK
   */
  getUser = (query: GetUserParams, params: RequestParams = {}) =>
    this.request<UserEntity, any>({
      path: `/api/users/get-user`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets a user by Keycloak ID
   *
   * @tags Users
   * @name getUserByKeycloakId
   * @summary Gets a user by Keycloak ID
   * @request GET:/api/users/get-user-by-keycloak-id
   * @secure
   * @response `200` `UserEntity` OK
   */
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
  /**
   * @description Searches users by search term
   *
   * @tags Users
   * @name searchUser
   * @summary Searches users by search term
   * @request GET:/api/users/search-user
   * @secure
   * @response `200` `(UserEntity)[]` OK
   */
  searchUser = (query: SearchUserParams, params: RequestParams = {}) =>
    this.request<UserEntity[], any>({
      path: `/api/users/search-user`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Synchronizes a user from Keycloak
   *
   * @tags Users
   * @name syncUser
   * @summary Synchronizes a user from Keycloak
   * @request POST:/api/users/sync-user
   * @secure
   * @response `200` `void` OK
   */
  syncUser = (data: SyncUserRequest, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/users/sync-user`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Starts synchronization of all users from Keycloak
   *
   * @tags Users
   * @name syncAllUsers
   * @summary Starts synchronization of all users from Keycloak
   * @request POST:/api/users/sync-all-users
   * @secure
   * @response `200` `void` OK
   */
  syncAllUsers = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/users/sync-all-users`,
      method: "POST",
      secure: true,
      ...params,
    });
}
