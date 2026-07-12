/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  FolderModuleSettings,
  GetModuleInstanceSettingsParams2,
} from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class FolderModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams2,
    params: RequestParams = {},
  ) =>
    this.request<FolderModuleSettings, ApiExceptionResponse>({
      path: `/api/folder-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
