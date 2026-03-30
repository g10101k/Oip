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
  /**
   * @description Retrieves Keycloak client settings needed by frontend applications.
   *
   * @tags Security
   * @name getKeycloakClientSettings
   * @summary Retrieves Keycloak client settings needed by frontend applications.
   * @request GET:/api/security/get-keycloak-client-settings
   * @secure
   * @response `200` `GetKeycloakClientSettingsResponse` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  getKeycloakClientSettings = (params: RequestParams = {}) =>
    this.request<GetKeycloakClientSettingsResponse, ApiExceptionResponse>({
      path: `/api/security/get-keycloak-client-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Retrieves all realm roles from Keycloak.
   *
   * @tags Security
   * @name getRealmRoles
   * @summary Retrieves all realm roles from Keycloak.
   * @request GET:/api/security/get-realm-roles
   * @secure
   * @response `200` `(string)[]` OK
   */
  getRealmRoles = (params: RequestParams = {}) =>
    this.request<string[], any>({
      path: `/api/security/get-realm-roles`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
