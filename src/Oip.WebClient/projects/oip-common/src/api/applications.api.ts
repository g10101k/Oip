/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  ApplicationRegistryItemDto,
  DeleteApplicationRegistryItemParams,
  GetApplicationRegistryItemByCodeParams,
  UpdateApplicationRegistryItemParams,
} from "./applications-data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable()
export class ApplicationsApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getApplicationRegistryItems = (params: RequestParams = {}) =>
    this.request<ApplicationRegistryItemDto[], ApiExceptionResponse>({
      path: `/api/applications/get-application-registry-items`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getApplicationRegistryItemByCode = (
    { code, ...query }: GetApplicationRegistryItemByCodeParams,
    params: RequestParams = {},
  ) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/applications/get-application-registry-item-by-code/${code}`,
      method: "GET",
      secure: true,
      ...params,
    });
  createApplicationRegistryItem = (
    data: ApplicationRegistryItemDto,
    params: RequestParams = {},
  ) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/applications/create-application-registry-item`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  updateApplicationRegistryItem = (
    { code, ...query }: UpdateApplicationRegistryItemParams,
    data: ApplicationRegistryItemDto,
    params: RequestParams = {},
  ) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/applications/update-application-registry-item/${code}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  deleteApplicationRegistryItem = (
    { code, ...query }: DeleteApplicationRegistryItemParams,
    params: RequestParams = {},
  ) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/applications/delete-application-registry-item/${code}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
}
