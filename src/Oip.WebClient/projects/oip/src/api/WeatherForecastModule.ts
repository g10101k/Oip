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
  PutSecurityRequest,
  SecurityResponse,
  WeatherForecastModuleGetModuleInstanceSettingsParams,
  WeatherForecastModuleGetSecurityParams,
  WeatherForecastModuleGetWeatherForecastParams,
  WeatherForecastResponse,
  WeatherModuleSettingsSaveSettingsRequest,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

@Injectable({ providedIn: "root" })
export class WeatherForecastModule<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags WeatherForecastModule
   * @name weatherForecastModuleGetWeatherForecast
   * @summary Get example data
   * @request GET:/api/weather-forecast-module/get-weather-forecast
   * @secure
   */
  weatherForecastModuleGetWeatherForecast = (
    query: WeatherForecastModuleGetWeatherForecastParams,
    params: RequestParams = {},
  ) =>
    this.request<WeatherForecastResponse[], any>({
      path: `/api/weather-forecast-module/get-weather-forecast`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecastModule
   * @name weatherForecastModuleGetModuleRights
   * @summary <inheritdoc />
   * @request GET:/api/weather-forecast-module/get-module-rights
   * @secure
   */
  weatherForecastModuleGetModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], any>({
      path: `/api/weather-forecast-module/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecastModule
   * @name weatherForecastModuleGetSecurity
   * @summary Get security for instance id
   * @request GET:/api/weather-forecast-module/get-security
   * @secure
   */
  weatherForecastModuleGetSecurity = (
    query: WeatherForecastModuleGetSecurityParams,
    params: RequestParams = {},
  ) =>
    this.request<SecurityResponse[], any>({
      path: `/api/weather-forecast-module/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecastModule
   * @name weatherForecastModulePutSecurity
   * @summary Update security
   * @request PUT:/api/weather-forecast-module/put-security
   * @secure
   */
  weatherForecastModulePutSecurity = (
    data: PutSecurityRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/weather-forecast-module/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecastModule
   * @name weatherForecastModuleGetModuleInstanceSettings
   * @summary Get instance setting
   * @request GET:/api/weather-forecast-module/get-module-instance-settings
   * @secure
   */
  weatherForecastModuleGetModuleInstanceSettings = (
    query: WeatherForecastModuleGetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/weather-forecast-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      ...params,
    });
  /**
   * No description
   *
   * @tags WeatherForecastModule
   * @name weatherForecastModulePutModuleInstanceSettings
   * @request PUT:/api/weather-forecast-module/put-module-instance-settings
   * @secure
   */
  weatherForecastModulePutModuleInstanceSettings = (
    data: WeatherModuleSettingsSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, any>({
      path: `/api/weather-forecast-module/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
