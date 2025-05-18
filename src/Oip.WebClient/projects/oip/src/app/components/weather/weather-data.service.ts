import { Injectable } from '@angular/core';
import { BaseDataService } from "oip-common";

export interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}



@Injectable()
export class WeatherDataService extends BaseDataService {
  getData(dayCount: number) {
    return this.sendRequest<WeatherForecast[]>(this.baseUrl + 'api/weather/get', 'GET', { dayCount: dayCount });
  }
}
