/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  ExternalModuleExampleDataDto,
  ExternalModuleExampleModuleSettings,
  GetModuleInstanceSettingsParams,
} from "./data-contracts";

@Injectable()
export class ExternalModuleExampleModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getExternalModuleExampleData = (params: RequestParams = {}) =>
    this.request<ExternalModuleExampleDataDto, ApiExceptionResponse>({
      path: `/api/external-module-example-module/get-external-module-example-data`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
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
