import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WeatherForecast } from '../../../dtos/weather.forecast'
import { TopBarItem, ModuleTopBarService } from 'shared-lib'
import { B } from "@fullcalendar/core/internal-common";

@Component({
  selector: 'weather-settings',
  templateUrl: './weather-settings.component.html'
})
export class WeatherSettingsComponent implements OnInit, OnDestroy {

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
  }

  ngOnDestroy(): void {
  }

  ngOnInit(): void {

  }

  contentClick() {
  }
}
