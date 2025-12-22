/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import { Injectable } from "@angular/core";
import {
  ApiExceptionResponse,
  GetKeycloakClientSettingsResponse,
} from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class Security<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Retrieves Keycloak client settings needed by frontend applications.
   *
   * @tags Security
   * @name securityGetKeycloakClientSettings
   * @request GET:/api/security/get-keycloak-client-settings
   * @secure
   */
  securityGetKeycloakClientSettings = (params: RequestParams = {}) =>
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
   * @name securityGetRealmRoles
   * @request GET:/api/security/get-realm-roles
   * @secure
   */
  securityGetRealmRoles = (params: RequestParams = {}) =>
    this.request<string[], any>({
      path: `/api/security/get-realm-roles`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
