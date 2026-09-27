/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  CompleteStepParams,
  CompleteUserStepRequest,
  CompleteUserStepResponse,
  GetStepByIdParams,
  UserStepDto,
} from "./data-contracts";

@Injectable()
export class WorkflowStepApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getStepById = (
    { workflowId, stepId, ...query }: GetStepByIdParams,
    params: RequestParams = {},
  ) =>
    this.request<UserStepDto, ApiExceptionResponse>({
      path: `/api/workflow-step/get-step-by-id/${workflowId}/${stepId}`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  completeStep = (
    { workflowId, stepId, ...query }: CompleteStepParams,
    data: CompleteUserStepRequest,
    params: RequestParams = {},
  ) =>
    this.request<CompleteUserStepResponse, ApiExceptionResponse>({
      path: `/api/workflow-step/complete-step/${workflowId}/${stepId}`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}
