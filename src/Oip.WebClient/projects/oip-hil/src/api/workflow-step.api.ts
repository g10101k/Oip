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
  GetStepAttachmentByNameParams,
  GetStepByIdParams,
  UploadStepFileParams,
  UploadStepFilePayload,
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
  uploadStepFile = (
    { workflowId, stepId, ...query }: UploadStepFileParams,
    data: UploadStepFilePayload,
    params: RequestParams = {},
  ) =>
    this.request<CompleteUserStepResponse, ApiExceptionResponse>({
      path: `/api/workflow-step/upload-step-file/${workflowId}/${stepId}`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.FormData,
      format: "json",
      ...params,
    });
  getStepAttachmentByName = (
    { workflowId, stepId, fileName, ...query }: GetStepAttachmentByNameParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/workflow-step/get-step-attachment-by-name/${workflowId}/${stepId}/${fileName}`,
      method: "GET",
      secure: true,
      ...params,
    });
}
