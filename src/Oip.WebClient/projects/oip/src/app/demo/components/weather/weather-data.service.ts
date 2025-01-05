import { inject, Injectable } from '@angular/core';
import { BaseDataService, PutSecurityDto, SecurityDto } from "common";
import { WeatherForecast } from "./dtos/weather.forecast";

@Injectable()
export class WeatherDataService extends BaseDataService {
  getData() {
    return this.sendRequest<WeatherForecast[]>(this.baseUrl + 'api/weather');
  }
}
