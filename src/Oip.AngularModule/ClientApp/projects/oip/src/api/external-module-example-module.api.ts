/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  ExternalModuleExampleModuleSettings,
  GetModuleInstanceSettingsParams,
} from "./data-contracts";

@Injectable()
export class ExternalModuleExampleModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<ExternalModuleExampleModuleSettings, ApiExceptionResponse>({
      path: `/api/external-module-example-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
