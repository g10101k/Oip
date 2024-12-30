import { inject, Injectable } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { OidcSecurityService } from "angular-auth-oidc-client";
import { BaseDataService } from "common";
import { WeatherForecast } from "./dtos/weather.forecast";

@Injectable()
export class WeatherDataService extends BaseDataService {

  getData() {
    return this.sendRequest<WeatherForecast[]>(this.baseUrl + 'api/weatherforecast');
  }
}
