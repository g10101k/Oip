import { Injectable } from '@angular/core';
import { BaseDataService } from "oip-common";
import { WeatherForecast } from "./dtos/weather.forecast";

@Injectable()
export class WeatherDataService extends BaseDataService {
  getData(dayCount: number) {
    return this.sendRequest<WeatherForecast[]>(this.baseUrl + 'api/weather/get', 'GET', { dayCount: dayCount });
  }
}
