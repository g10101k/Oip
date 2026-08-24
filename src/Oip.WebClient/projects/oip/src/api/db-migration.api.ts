/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  ApplyMigrationRequest,
  MigrationDto,
} from "./data-contracts";

@Injectable()
export class DbMigrationApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getMigrations = (params: RequestParams = {}) =>
    this.request<MigrationDto[], ApiExceptionResponse>({
      path: `/api/db-migration/get-migrations`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  migrate = (params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/db-migration/migrate`,
      method: "POST",
      secure: true,
      ...params,
    });
  applyMigration = (data: ApplyMigrationRequest, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/db-migration/apply-migration`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  getModuleInstanceSettings = (params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/db-migration/get-module-instance-settings`,
      method: "GET",
      secure: true,
      ...params,
    });
}
