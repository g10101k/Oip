/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  GetStepsByPeriodRequest,
  UserStepDto,
  WorkflowActivityModuleSettings,
} from "./data-contracts";

@Injectable()
export class WorkflowActivityModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getStepsByPeriod = (
    data: GetStepsByPeriodRequest,
    params: RequestParams = {},
  ) =>
    this.request<UserStepDto[], ApiExceptionResponse>({
      path: `/api/workflow-activity-module/get-steps-by-period`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  getModuleInstanceSettings = (params: RequestParams = {}) =>
    this.request<WorkflowActivityModuleSettings, ApiExceptionResponse>({
      path: `/api/workflow-activity-module/get-module-instance-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
