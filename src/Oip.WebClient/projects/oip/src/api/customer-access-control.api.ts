/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  AccessObjectTypeDto,
  ApiExceptionResponse,
  GetEffectiveRightsByObjectParams,
  GetObjectAclByObjectParams,
  GetSubjectsByFilterParams,
  ObjectAclDto,
  SubjectDto,
  UpdateObjectAclRequest,
} from "./data-contracts";

@Injectable()
export class CustomerAccessControlApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getObjectTypes = (params: RequestParams = {}) =>
    this.request<AccessObjectTypeDto[], ApiExceptionResponse>({
      path: `/api/customer-access-control/get-object-types`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getObjectAclByObject = (
    query: GetObjectAclByObjectParams,
    params: RequestParams = {},
  ) =>
    this.request<ObjectAclDto, ApiExceptionResponse>({
      path: `/api/customer-access-control/get-object-acl-by-object`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  updateObjectAcl = (
    data: UpdateObjectAclRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/customer-access-control/update-object-acl`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  getEffectiveRightsByObject = (
    query: GetEffectiveRightsByObjectParams,
    params: RequestParams = {},
  ) =>
    this.request<string[], ApiExceptionResponse>({
      path: `/api/customer-access-control/get-effective-rights-by-object`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  getSubjectsByFilter = (
    query: GetSubjectsByFilterParams,
    params: RequestParams = {},
  ) =>
    this.request<SubjectDto[], ApiExceptionResponse>({
      path: `/api/customer-access-control/get-subjects-by-filter`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
