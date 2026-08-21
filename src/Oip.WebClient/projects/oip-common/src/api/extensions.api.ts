/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ApiExceptionResponse } from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class ExtensionsApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getModuleInstanceSettings = (params: RequestParams = {}) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/extensions/get-module-instance-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
