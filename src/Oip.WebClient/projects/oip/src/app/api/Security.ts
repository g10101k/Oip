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

import { GetKeycloakClientSettingsResponse } from "./data-contracts";
import { HttpClient, RequestParams } from "./http-client";

export class Security<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Security
   * @name securityGetRealmRolesList
   * @summary Get all roles
   * @request GET:/api/security/get-realm-roles
   * @secure
   */
  securityGetRealmRolesList = (params: RequestParams = {}) =>
    this.request<string[], any>({
      path: `/api/security/get-realm-roles`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Security
   * @name securityGetKeycloakClientSettingsList
   * @summary Get keycloak client settings
   * @request GET:/api/security/get-keycloak-client-settings
   * @secure
   */
  securityGetKeycloakClientSettingsList = (params: RequestParams = {}) =>
    this.request<GetKeycloakClientSettingsResponse, any>({
      path: `/api/security/get-keycloak-client-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
