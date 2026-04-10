import { Component, inject, OnDestroy, OnInit, output, ViewChild } from '@angular/core';
import { BaseModuleComponent, L10nService, SecurityComponent } from 'oip-common';
import { WeatherForecastModuleApi } from '../../../api/weather-forecast-module.api';
import { WeatherForecastResponse, WeatherModuleSettings } from '../../../api/data-contracts';
import { TagModule } from 'primeng/tag';
import { FilterMetadata, SharedModule } from 'primeng/api';
import { Table, TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { DatePipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

interface WeatherModuleLocalSettings {
  first: number;
  rows: number;
  filters: {
    [p: string]: FilterMetadata | FilterMetadata[];
  };
}

@Component({
  template: `
    @if (isContent) {
      <div class="card">
        <div>
          <h5>{{ this.title }}</h5>
          <p-table #table [value]="data" (onFilter)="onFilter()">
            <ng-template let-columns pTemplate="header">
              <tr>
                <th pSortableColumn="date" scope="col">
                  {{ 'weather-forecast-module.content.table.date' | translate }}
                  <p-sortIcon field="date"/>
                  <p-columnFilter display="menu" field="date" type="date"/>
                </th>
                <th pSortableColumn="temperatureC" scope="col">
                  {{ 'weather-forecast-module.content.table.temperatureC' | translate }}
                  <p-sortIcon field="temperatureC"/>
                  <p-columnFilter display="menu" field="temperatureC" type="numeric"/>
                </th>
                <th pSortableColumn="temperatureF" scope="col">
                  {{ 'weather-forecast-module.content.table.temperatureF' | translate }}
                  <p-sortIcon field="temperatureF"/>
                  <p-columnFilter display="menu" field="temperatureF" type="numeric"/>
                </th>
                <th pSortableColumn="summary" scope="col">
                  {{ 'weather-forecast-module.content.table.summary' | translate }}
                  <p-sortIcon field="summary"/>
                  <p-columnFilter display="menu" field="summary" type="text"/>
                </th>
              </tr>
            </ng-template>
            <ng-template let-columns="columns" let-forecast pTemplate="body">
              <tr>
                <td>{{ forecast.date | date: layoutService.dateTimeFormat() }}</td>
                <td>{{ forecast.temperatureC }}</td>
                <td>{{ forecast.temperatureF }}</td>
                <td>
                  <p-tag severity="success" [value]="forecast.summary"></p-tag>
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>
    } @else if (isSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'weather-forecast-module.settings.title' | translate }}</div>
            <div class="grid grid-cols-12 gap-4">
              <label class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0" for="dayCount">
                {{ 'weather-forecast-module.settings.dayCount' | translate }}
              </label>
              <div class="col-span-12 md:col-span-10">
                <input id="dayCount" pInputText type="text" [(ngModel)]="settings.dayCount"/>
              </div>
            </div>
            <div class="flex justify-end">
              <p-button
                icon="pi pi-save"
                [label]="'weather-forecast-module.settings.save' | translate"
                (onClick)="saveSettings(settings)"></p-button>
            </div>
          </div>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `,
  providers: [WeatherForecastModuleApi],
  imports: [TableModule, SharedModule, TagModule, SecurityComponent, Button, FormsModule, InputText, DatePipe, TranslatePipe]
})
export class WeatherForecastModuleComponent
  extends BaseModuleComponent<WeatherModuleSettings, WeatherModuleLocalSettings>
  implements OnInit, OnDestroy {
  @ViewChild('table') table!: Table;
  protected readonly dataService = inject(WeatherForecastModuleApi);
  protected readonly l10nService = inject(L10nService);
  protected data: WeatherForecastResponse[] = [];

  constructor() {
    super();
    this.l10nService.get('weather-forecast-module').subscribe((q) => {
      this.appTitleService.setTitle(q.title);
    });
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    try {
      this.data = await this.dataService.getWeatherForecast({
        dayCount: this.settings.dayCount
      });
    } catch (error) {
      this.data = [];
      console.error('Error fetching weather data:', error);
      this.msgService.error(error.error.message);
    }
    if (this.localSettings().filters && this.table) this.table.filters = this.localSettings().filters;
  }

  onFilter() {
    this.localSettings.update((settings) => ({
      ...settings,
      filters: this.table.filters
    }));
  }
}
