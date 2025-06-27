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
import { FolderGetSecurityParams, SecurityResponse } from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class FolderModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags FolderModule
   * @name folderGetSecurity
   * @summary Get security for instance id
   * @request GET:/api/folder/get-security
   * @secure
   */
  folderGetSecurity = (
    query: FolderGetSecurityParams,
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/folder/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags FolderModule
   * @name folderGetModuleRights
   * @request GET:/api/folder/get-module-rights
   * @secure
   */
  folderGetModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/folder/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
