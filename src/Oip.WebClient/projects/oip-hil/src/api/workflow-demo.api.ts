/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  RunUserTaskDemoResponse,
  RunWorkflowResponse,
} from "./data-contracts";

@Injectable()
export class WorkflowDemoApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  runHelloWorld = (params: RequestParams = {}) =>
    this.request<RunWorkflowResponse, ApiExceptionResponse>({
      path: `/api/workflow-demo/run-hello-world`,
      method: "POST",
      secure: true,
      format: "json",
      ...params,
    });
  runUserTaskDemo = (params: RequestParams = {}) =>
    this.request<RunUserTaskDemoResponse, ApiExceptionResponse>({
      path: `/api/workflow-demo/run-user-task-demo`,
      method: "POST",
      secure: true,
      format: "json",
      ...params,
    });
}
