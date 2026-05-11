/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ApiExceptionResponse, CryptRequest } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class CryptApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  protect = (data: CryptRequest, params: RequestParams = {}) =>
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
