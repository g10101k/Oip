import { inject, Injectable } from '@angular/core';
import { BaseDataService, PutSecurityDto, SecurityDto } from "common";
import { WeatherForecast } from "./dtos/weather.forecast";

@Injectable()
export class WeatherDataService extends BaseDataService {

  getData() {
    return this.sendRequest<WeatherForecast[]>(this.baseUrl + 'api/weatherforecast');
  }

  getSecurity(id: number) {
    return this.sendRequest<SecurityDto[]>(this.baseUrl + `api/weatherforecast/get-security?id=${id}`);
  }

  saveSecurity(request: PutSecurityDto) {
    return this.sendRequest<any>(this.baseUrl + `api/weatherforecast/put-security`, 'PUT', request);
  }
}
