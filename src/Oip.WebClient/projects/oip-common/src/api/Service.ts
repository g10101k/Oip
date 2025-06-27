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
import { GetManifestResponse, RegisterModuleDto } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class Service<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Service
   * @name serviceGet
   * @summary Get manifest for client app
   * @request GET:/api/service/get
   * @secure
   */
  serviceGet = (params: RequestParams = {}) =>
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
   * @name serviceRegisterModule
   * @summary Registry module
   * @request POST:/api/service/register-module
   * @secure
   */
  serviceRegisterModule = (
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
