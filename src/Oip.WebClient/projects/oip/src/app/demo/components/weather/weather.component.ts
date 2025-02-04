import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BaseComponent, Feature, SecurityComponent } from 'oip-common'
import { WeatherDataService, WeatherForecast } from "./weather-data.service";
import { WeatherSettingsComponent, WeatherSettingsDto } from './weather-settings.component';
import { TagModule } from 'primeng/tag';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { NgIf } from '@angular/common';



@Component({
    selector: 'weather',
    template: `<div *ngIf="isContent" class="card">
      <div>
        <h5>Weather</h5>
        <p-table [value]="data" [tableStyle]="{'min-width': '50rem'}">
          <ng-template pTemplate="header" let-columns>
            <tr>
              <th scope="col">Date</th>
              <th scope="col">Temperature C</th>
              <th scope="col">Temperature F</th>
              <th scope="col">Summary</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-forecast let-columns="columns">
            <tr>
              <td>{{ forecast.date }}</td>
              <td>{{ forecast.temperatureC }}</td>
              <td>{{ forecast.temperatureF }}</td>
              <td>
                <p-tag [value]="forecast.summary" severity="success"></p-tag>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    </div>
    <weather-settings *ngIf="isSettings" [(settings)]="settings" (settingsChange)="onSettingsChange($event)"></weather-settings>
    <security *ngIf="isSecurity" [id]="id" [controller]="controller"></security>
    `,
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
