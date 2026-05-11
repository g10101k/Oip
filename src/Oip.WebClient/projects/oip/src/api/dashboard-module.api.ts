/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  DashboardGetModuleInstanceSettingsParams,
  DashboardSettings,
} from "./data-contracts";

@Injectable()
export class DashboardModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  dashboardGetModuleInstanceSettings = (
    query: DashboardGetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<DashboardSettings, ApiExceptionResponse>({
      path: `/api/dashboard/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
