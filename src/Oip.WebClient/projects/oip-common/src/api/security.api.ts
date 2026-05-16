/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  AuthCsrfTokenResponse,
  AuthSessionResponse,
  GetKeycloakClientSettingsResponse,
} from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class SecurityApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getCurrentAuthSession = (params: RequestParams = {}) =>
    this.request<AuthSessionResponse, ApiExceptionResponse>({
      path: `/api/security/get-current-auth-session`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  createAuthSession = (params: RequestParams = {}) =>
    this.request<any, void | ApiExceptionResponse>({
      path: `/api/security/create-auth-session`,
      method: "POST",
      secure: true,
      ...params,
    });
  deleteAuthSession = (params: RequestParams = {}) =>
    this.request<any, void | ApiExceptionResponse>({
      path: `/api/security/delete-auth-session`,
      method: "POST",
      secure: true,
      ...params,
    });
  getAuthCsrfToken = (params: RequestParams = {}) =>
    this.request<AuthCsrfTokenResponse, ApiExceptionResponse>({
      path: `/api/security/get-auth-csrf-token`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
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
