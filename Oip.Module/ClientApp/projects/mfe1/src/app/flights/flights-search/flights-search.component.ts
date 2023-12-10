import {Component, Inject} from '@angular/core';
import {AuthLibService} from 'auth-lib';
import {HttpClient} from '@angular/common/http';
import {BaseUrlService} from "../../../services/base-url.service";


@Component({
  selector: 'app-flights-search',
  templateUrl: './flights-search.component.html'
})
export class FlightsSearchComponent {
  public forecasts: WeatherForecast[] = [];

  constructor(http: HttpClient, private service: AuthLibService, baseUrl: BaseUrlService) {
    console.log('User Name', this.service.user);
    console.log(baseUrl.getUrl());

    http.get<WeatherForecast[]>(baseUrl.getUrl() + 'weatherforecast').subscribe(result => {
      this.forecasts = result;
    }, error => console.error(error));
  }
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}
