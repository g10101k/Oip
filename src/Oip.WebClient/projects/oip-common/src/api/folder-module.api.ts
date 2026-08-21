/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ApiExceptionResponse, FolderModuleSettings } from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class FolderModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getModuleInstanceSettings = (params: RequestParams = {}) =>
    this.request<FolderModuleSettings, ApiExceptionResponse>({
      path: `/api/folder-module/get-module-instance-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
