/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  DemoCustomerTableRowDtoTablePageResult,
  TableQueryRequest,
} from "./data-contracts";

@Injectable()
export class DemoCustomerTableApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Retrieves a filtered page of rows for PrimeNG p-table lazy loading.
   *
   * @tags DemoCustomerTable
   * @name getPage
   * @summary Retrieves a filtered page of rows for PrimeNG p-table lazy loading.
   * @request POST:/get-page
   * @secure
   * @response `200` `DemoCustomerTableRowDtoTablePageResult` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  getPage = (data: TableQueryRequest, params: RequestParams = {}) =>
    this.request<DemoCustomerTableRowDtoTablePageResult, ApiExceptionResponse>({
      path: `/get-page`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}
