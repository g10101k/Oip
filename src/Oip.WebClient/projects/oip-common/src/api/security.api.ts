/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  GetKeycloakClientSettingsResponse,
} from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class SecurityApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getKeycloakClientSettings = (params: RequestParams = {}) =>
    this.request<GetKeycloakClientSettingsResponse, ApiExceptionResponse>({
      path: `/api/security/get-keycloak-client-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getRealmRoles = (params: RequestParams = {}) =>
    this.request<string[], any>({
      path: `/api/security/get-realm-roles`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
