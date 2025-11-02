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
  SyncUserRequest,
  UserEntity,
  UsersGetAllUsersParams,
  UsersGetUserByKeycloakIdParams,
  UsersGetUserParams,
  UsersSearchUserParams,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class Users<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Users
   * @name usersGetAllUsers
   * @summary Gets a list of users with pagination
   * @request GET:/api/users/get-all-users
   * @secure
   */
  usersGetAllUsers = (
    query: UsersGetAllUsersParams,
    params: RequestParams = {},
  ) =>
    this.request<UserEntity[], any>({
      path: `/api/users/get-all-users`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name usersGetUser
   * @summary Gets a user by ID
   * @request GET:/api/users/get-user
   * @secure
   */
  usersGetUser = (query: UsersGetUserParams, params: RequestParams = {}) =>
    this.request<UserEntity, any>({
      path: `/api/users/get-user`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name usersGetUserByKeycloakId
   * @summary Gets a user by Keycloak ID
   * @request GET:/api/users/get-user-by-keycloak-id
   * @secure
   */
  usersGetUserByKeycloakId = (
    query: UsersGetUserByKeycloakIdParams,
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
   * No description
   *
   * @tags Users
   * @name usersSearchUser
   * @summary Searches users by search term
   * @request GET:/api/users/search-user
   * @secure
   */
  usersSearchUser = (
    query: UsersSearchUserParams,
    params: RequestParams = {},
  ) =>
    this.request<UserEntity[], any>({
      path: `/api/users/search-user`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name usersSyncUser
   * @summary Synchronizes a user from Keycloak
   * @request POST:/api/users/sync-user
   * @secure
   */
  usersSyncUser = (data: SyncUserRequest, params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/users/sync-user`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name usersSyncAllUsers
   * @summary Starts synchronization of all users from Keycloak
   * @request POST:/api/users/sync-all-users
   * @secure
   */
  usersSyncAllUsers = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/users/sync-all-users`,
      method: "POST",
      secure: true,
      ...params,
    });
}
