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

import { GetManifestResponse, RegisterModuleDto } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class Service<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Service
   * @name getList
   * @summary Get manifest for client app
   * @request GET:/api/service/get
   * @secure
   */
  serviceGetList = (params: RequestParams = {}) =>
    this.request<Record<string, GetManifestResponse>, any>({
      path: `/api/service/get`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Service
   * @name registerModuleCreate
   * @summary Registry module
   * @request POST:/api/service/register-module
   * @secure
   */
  serviceRegisterModuleCreate = (
    data: RegisterModuleDto,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/service/register-module`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
