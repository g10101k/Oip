/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  CreateUserExtensionFieldRequest,
  DeleteUserExtensionFieldParams,
  ExtensionFieldMetadataDto,
  GetModuleInstanceSettingsParams2,
  TableQueryRequest,
  UpdateUserExtensionFieldParams,
  UpdateUserExtensionFieldRequest,
  UpdateUserExtensionValuesParams,
  UpdateUserExtensionValuesRequest,
  UserExtensionModuleSettings,
  UserExtensionTableRowDtoExtensionTablePageResult,
} from "./data-contracts";

@Injectable()
export class UserExtensionModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getUserExtensionPage = (
    data: TableQueryRequest,
    params: RequestParams = {},
  ) =>
    this.request<
      UserExtensionTableRowDtoExtensionTablePageResult,
      ApiExceptionResponse
    >({
      path: `/api/user-extension-module/get-user-extension-page`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  getUserExtensionFields = (params: RequestParams = {}) =>
    this.request<ExtensionFieldMetadataDto[], ApiExceptionResponse>({
      path: `/api/user-extension-module/get-user-extension-fields`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  createUserExtensionField = (
    data: CreateUserExtensionFieldRequest,
    params: RequestParams = {},
  ) =>
    this.request<ExtensionFieldMetadataDto, ApiExceptionResponse>({
      path: `/api/user-extension-module/create-user-extension-field`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  updateUserExtensionField = (
    { id, ...query }: UpdateUserExtensionFieldParams,
    data: UpdateUserExtensionFieldRequest,
    params: RequestParams = {},
  ) =>
    this.request<ExtensionFieldMetadataDto, ApiExceptionResponse>({
      path: `/api/user-extension-module/update-user-extension-field/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  deleteUserExtensionField = (
    { id, ...query }: DeleteUserExtensionFieldParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/user-extension-module/delete-user-extension-field/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  updateUserExtensionValues = (
    { userId, ...query }: UpdateUserExtensionValuesParams,
    data: UpdateUserExtensionValuesRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/user-extension-module/update-user-extension-values/${userId}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams2,
    params: RequestParams = {},
  ) =>
    this.request<UserExtensionModuleSettings, ApiExceptionResponse>({
      path: `/api/user-extension-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
