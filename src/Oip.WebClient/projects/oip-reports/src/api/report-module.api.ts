/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  ReportDataSourceSchema,
  ReportDefinition,
  ReportDefinitionSummary,
  ReportDeleteReportParams,
  ReportExportResult,
  ReportGetModuleInstanceSettingsParams,
  ReportGetReportByIdParams,
  ReportGetReportDataSourceSchemaByReportIdParams,
  ReportModuleSettings,
  ReportRequest,
  ReportResult,
  ReportUpdateReportParams,
} from "./data-contracts";

@Injectable()
export class ReportModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  reportGetReports = (params: RequestParams = {}) =>
    this.request<ReportDefinitionSummary[], ApiExceptionResponse>({
      path: `/api/report/get-reports`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  reportCreateReport = (data: ReportDefinition, params: RequestParams = {}) =>
    this.request<ReportDefinition, ApiExceptionResponse>({
      path: `/api/report/create-report`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  reportGetReportPreview = (data: ReportRequest, params: RequestParams = {}) =>
    this.request<ReportResult, ApiExceptionResponse>({
      path: `/api/report/get-report-preview`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  reportGetReportExport = (data: ReportRequest, params: RequestParams = {}) =>
    this.request<ReportExportResult, ApiExceptionResponse>({
      path: `/api/report/get-report-export`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  reportGetReportById = (
    query: ReportGetReportByIdParams,
    params: RequestParams = {},
  ) =>
    this.request<ReportDefinition, ApiExceptionResponse>({
      path: `/api/report/get-report-by-id`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  reportGetReportDataSourceSchemaByReportId = (
    query: ReportGetReportDataSourceSchemaByReportIdParams,
    params: RequestParams = {},
  ) =>
    this.request<ReportDataSourceSchema[], ApiExceptionResponse>({
      path: `/api/report/get-report-data-source-schema-by-report-id`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  reportUpdateReport = (
    { id, ...query }: ReportUpdateReportParams,
    data: ReportDefinition,
    params: RequestParams = {},
  ) =>
    this.request<ReportDefinition, ApiExceptionResponse>({
      path: `/api/report/update-report/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  reportDeleteReport = (
    { id, ...query }: ReportDeleteReportParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/report/delete-report/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  reportGetModuleInstanceSettings = (
    query: ReportGetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<ReportModuleSettings, ApiExceptionResponse>({
      path: `/api/report/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
