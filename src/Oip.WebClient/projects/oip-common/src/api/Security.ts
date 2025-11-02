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
import { GetKeycloakClientSettingsResponse } from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class Security<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description This endpoint is restricted to administrators. Useful for role management in the application UI or backend.
   *
   * @tags Security
   * @name securityGetRealmRoles
   * @summary Retrieves all realm roles from Keycloak.
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
  /**
   * @description This endpoint is publicly accessible and provides client configuration such as authority URL, client ID, scopes, and secure routes for frontend OAuth2/OIDC initialization.
   *
   * @tags Security
   * @name securityGetKeycloakClientSettings
   * @summary Retrieves Keycloak client settings needed by frontend applications.
   * @request GET:/api/security/get-keycloak-client-settings
   * @secure
   */
  securityGetKeycloakClientSettings = (params: RequestParams = {}) =>
    this.request<GetKeycloakClientSettingsResponse, any>({
      path: `/api/security/get-keycloak-client-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
