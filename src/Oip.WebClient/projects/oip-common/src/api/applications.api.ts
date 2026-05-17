/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ApplicationRegistryItemDto } from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class ApplicationsApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  get = (params: RequestParams = {}) =>
    this.request<ApplicationRegistryItemDto[], any>({
      path: `/api/applications/get`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
