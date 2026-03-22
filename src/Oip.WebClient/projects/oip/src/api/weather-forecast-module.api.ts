import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  GetModuleInstanceSettingsParams,
  GetSecurityParams,
  GetWeatherForecastParams,
  PutSecurityRequest,
  SecurityResponse,
  WeatherForecastResponse,
  WeatherModuleSettingsSaveSettingsRequest,
} from "./data-contracts";

@Injectable()
export class WeatherForecastModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Retrieves example weather forecast data.
   *
   * @tags WeatherForecastModule
   * @name getWeatherForecast
   * @summary Retrieves example weather forecast data.
   * @request GET:/api/weather-forecast-module/get-weather-forecast
   * @secure
   * @response `200` `(WeatherForecastResponse)[]` OK
   * @response `500` `ApiExceptionResponse` Internal Server Error
   */
  getWeatherForecast = (
    query: GetWeatherForecastParams,
    params: RequestParams = {},
  ) =>
    this.request<WeatherForecastResponse[], ApiExceptionResponse>({
      path: `/api/weather-forecast-module/get-weather-forecast`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description <inheritdoc />
   *
   * @tags WeatherForecastModule
   * @name getModuleRights
   * @summary <inheritdoc />
   * @request GET:/api/weather-forecast-module/get-module-rights
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getModuleRights = (params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/weather-forecast-module/get-module-rights`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Gets the security configuration for the specified module instance ID.
   *
   * @tags WeatherForecastModule
   * @name getSecurity
   * @summary Gets the security configuration for the specified module instance ID.
   * @request GET:/api/weather-forecast-module/get-security
   * @secure
   * @response `200` `(SecurityResponse)[]` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getSecurity = (query: GetSecurityParams, params: RequestParams = {}) =>
    this.request<SecurityResponse[], ApiExceptionResponse>({
      path: `/api/weather-forecast-module/get-security`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
  /**
   * @description Updates the security configuration for the specified module instance.
   *
   * @tags WeatherForecastModule
   * @name putSecurity
   * @summary Updates the security configuration for the specified module instance.
   * @request PUT:/api/weather-forecast-module/put-security
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  putSecurity = (data: PutSecurityRequest, params: RequestParams = {}) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/weather-forecast-module/put-security`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
  /**
   * @description Gets the settings for the specified module instance.
   *
   * @tags WeatherForecastModule
   * @name getModuleInstanceSettings
   * @summary Gets the settings for the specified module instance.
   * @request GET:/api/weather-forecast-module/get-module-instance-settings
   * @secure
   * @response `200` `any` OK
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams,
    params: RequestParams = {},
  ) =>
    this.request<any, ApiExceptionResponse>({
      path: `/api/weather-forecast-module/get-module-instance-settings`,
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
   * @name putModuleInstanceSettings
   * @request PUT:/api/weather-forecast-module/put-module-instance-settings
   * @secure
   * @response `200` `void` OK
   * @response `400` `ApiExceptionResponse` Bad Request
   * @response `401` `ApiExceptionResponse` Unauthorized
   * @response `403` `ApiExceptionResponse` Forbidden
   */
  putModuleInstanceSettings = (
    data: WeatherModuleSettingsSaveSettingsRequest,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/weather-forecast-module/put-module-instance-settings`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      ...params,
    });
}
