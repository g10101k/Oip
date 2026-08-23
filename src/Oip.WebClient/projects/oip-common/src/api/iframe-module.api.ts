/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ApiExceptionResponse, IframeModuleSettings } from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class IframeModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getModuleInstanceSettings = (params: RequestParams = {}) =>
    this.request<IframeModuleSettings, ApiExceptionResponse>({
      path: `/api/iframe-module/get-module-instance-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
