import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { WeatherForecast } from './dtos/weather.forecast'
import { BaseComponent, Feature, SecurityComponent } from 'oip-common'
import { WeatherDataService } from "./weather-data.service";
import { WeatherSettingsDto } from "./dtos/weather-settings.dto";
import { WeatherSettingsComponent } from './weather-settings/weather-settings.component';
import { TagModule } from 'primeng/tag';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { NgIf } from '@angular/common';

@Component({
    selector: 'weather',
    templateUrl: './weather.component.html',
    providers: [{ provide: 'controller', useValue: 'weather' }, WeatherDataService],
    standalone: true,
    imports: [
        NgIf,
        TableModule,
        SharedModule,
        TagModule,
        WeatherSettingsComponent,
        SecurityComponent,
    ],
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
