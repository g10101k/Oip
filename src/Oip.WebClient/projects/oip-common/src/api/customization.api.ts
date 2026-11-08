/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ApiExceptionResponse, SiteCustomization } from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class CustomizationApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getCustomization = (params: RequestParams = {}) =>
    this.request<SiteCustomization, ApiExceptionResponse>({
      path: `/api/customization/get-customization`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  updateCustomization = (data: SiteCustomization, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/customization/update-customization`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
