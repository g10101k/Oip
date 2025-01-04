import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { WeatherForecast } from './dtos/weather.forecast'
import { BaseComponent } from 'common'
import { WeatherDataService } from "./weather-data.service";

@Component({
  selector: 'weather',
  templateUrl: './weather.component.html'
})
export class WeatherComponent extends BaseComponent implements OnInit, OnDestroy {
  protected readonly dataService: WeatherDataService = inject(WeatherDataService);
  protected forecasts: WeatherForecast[] = [];

  constructor() {
    super();
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  ngOnInit(): void {
    this.dataService.getData().then(result => {
      this.forecasts = result;
    }, error => this.msgService.error(error));
    super.ngOnInit();
  }
}
