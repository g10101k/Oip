import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { WeatherForecast } from './dtos/weather.forecast'
import { BaseComponent } from 'common'
import { WeatherDataService } from "./weather-data.service";
import { NgIf } from '@angular/common';
import { TableModule } from 'primeng/table';
import { PrimeTemplate } from 'primeng/api';
import { Tag } from 'primeng/tag';
import { WeatherSettingsComponent } from './weather-settings/weather-settings.component';
import { WeatherSecurityComponent } from './weather-security/weather-security.component';

@Component({
    selector: 'weather',
    templateUrl: './weather.component.html',
    imports: [NgIf, TableModule, PrimeTemplate, Tag, WeatherSettingsComponent, WeatherSecurityComponent]
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
