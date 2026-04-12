/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { HttpClient, RequestParams } from "./http-client";

@Injectable()
export class ProxySettingsApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Retrieves the current proxy configuration settings for the application.
   *
   * @tags ProxySettings
   * @name getSpaProxySettings
   * @summary Retrieves the current proxy configuration settings for the application.
   * @request GET:/api/proxy-settings/get-spa-proxy-settings
   * @secure
   * @response `200` `void` OK
   */
  getSpaProxySettings = (params: RequestParams = {}) =>
    this.request<void, any>({
      path: `/api/proxy-settings/get-spa-proxy-settings`,
      method: "GET",
      secure: true,
      ...params,
    });
}
