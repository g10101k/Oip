/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  GetModuleInstanceSettingsParams4,
  IframeModuleSettings,
} from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class IframeModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams4,
    params: RequestParams = {},
  ) =>
    this.request<IframeModuleSettings, ApiExceptionResponse>({
      path: `/api/iframe-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
