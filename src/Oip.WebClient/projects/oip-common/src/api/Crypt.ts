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
import { ApiExceptionResponse, CryptRequest } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class Crypt<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Protects a message using encryption services.
   *
   * @tags Crypt
   * @name cryptProtect
   * @request POST:/api/crypt/protect
   * @secure
   */
  cryptProtect = (data: CryptRequest, params: RequestParams = {}) =>
    this.request<string, ApiExceptionResponse>({
      path: `/api/crypt/protect`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}
