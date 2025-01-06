import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { WeatherForecast } from './dtos/weather.forecast'
import { BaseComponent, Feature } from 'oip/common'
import { WeatherDataService } from "./weather-data.service";
import { WeatherSettingsDto } from "./dtos/weather-settings.dto";

@Component({
  selector: 'weather',
  templateUrl: './weather.component.html',
  providers: [{ provide: 'controller', useValue: 'weather' }],
})
export class WeatherComponent extends BaseComponent<WeatherSettingsDto> implements OnInit, OnDestroy, Feature {
  controller: string = 'weather';
  protected readonly dataService: WeatherDataService = inject(WeatherDataService);
  protected data: WeatherForecast[] = [];

  constructor() {
    super();
  }

  async ngOnInit() {
    await super.ngOnInit();
    await this.dataService.getData(this.settings.dayCount).then(result => {
      this.data = result;
    }, error => this.msgService.error(error));
  }

  async onSettingsChange($event: WeatherSettingsDto) {
    await this.saveSettings($event)
  }
}
