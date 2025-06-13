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

import {
  PutSecurityRequest,
  SecurityResponse,
  WeatherForecastResponse,
  WeatherGetListParams,
  WeatherGetModuleInstanceSettingsListParams,
  WeatherGetSecurityListParams,
  WeatherModuleSettingsSaveSettingsRequest,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";
import { Injectable } from "@angular/core";

@Injectable()
export class WeatherForecast<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags WeatherForecast
   * @name weatherGetList
   * @summary Get example data
   * @request GET:/api/weather/get
   * @secure
   */
  weatherGetList = (query: WeatherGetListParams, params: RequestParams = {}) =>
    this.request<WeatherForecastResponse[], any>({
      path: `/api/weather/get`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecast
   * @name weatherGetModuleRightsList
   * @summary <inheritdoc />
   * @request GET:/api/weather/get-module-rights
   * @secure
   */
  weatherGetModuleRightsList = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/weather/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecast
   * @name weatherGetSecurityList
   * @summary Get security for instance id
   * @request GET:/api/weather/get-security
   * @secure
   */
  weatherGetSecurityList = (
    query: WeatherGetSecurityListParams,
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/weather/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecast
   * @name weatherPutSecurityUpdate
   * @summary Update security
   * @request PUT:/api/weather/put-security
   * @secure
   */
  weatherPutSecurityUpdate = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/weather/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecast
   * @name weatherGetModuleInstanceSettingsList
   * @summary Get instance setting
   * @request GET:/api/weather/get-module-instance-settings
   * @secure
   */
  weatherGetModuleInstanceSettingsList = (
    query: WeatherGetModuleInstanceSettingsListParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/weather/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecast
   * @name weatherPutModuleInstanceSettingsUpdate
   * @request PUT:/api/weather/put-module-instance-settings
   * @secure
   */
  weatherPutModuleInstanceSettingsUpdate = (
    data: WeatherModuleSettingsSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/weather/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
