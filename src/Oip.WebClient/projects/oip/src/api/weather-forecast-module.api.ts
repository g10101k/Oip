/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { HttpClient, RequestParams } from "oip-common";
import {
  ApiExceptionResponse,
  GetModuleInstanceSettingsParams4,
  GetWeatherForecastParams,
  WeatherForecastResponse,
  WeatherModuleSettings,
} from "./data-contracts";

@Injectable()
export class WeatherForecastModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
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
  getModuleInstanceSettings = (
    query: GetModuleInstanceSettingsParams4,
    params: RequestParams = {},
  ) =>
    this.request<WeatherModuleSettings, ApiExceptionResponse>({
      path: `/api/weather-forecast-module/get-module-instance-settings`,
      method: "GET",
      query: query,
      secure: true,
      format: "json",
      ...params,
    });
}
