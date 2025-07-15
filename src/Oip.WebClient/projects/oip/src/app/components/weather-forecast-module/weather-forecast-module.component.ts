import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { BaseModuleComponent, SecurityComponent } from 'oip-common'
import { WeatherForecastModule } from "../../../api/WeatherForecastModule";
import { WeatherForecastResponse, WeatherModuleSettings } from "../../../api/data-contracts";
import { TagModule } from 'primeng/tag';
import { FilterMetadata, SharedModule } from 'primeng/api';
import { Table, TableModule } from 'primeng/table';
import { NgIf } from '@angular/common';
import { Button } from "primeng/button";
import { FormsModule } from "@angular/forms";
import { InputText } from "primeng/inputtext";

interface WeatherModuleLocalSettings {
  first: number;
  rows: number;
  filters: {
    [p: string]: FilterMetadata | FilterMetadata[],
  };
}

@Component({
  selector: 'weather-forecast-module',
  template: `
    <div *ngIf="isContent" class="card">
      <div>
        <h5>{{ this.title }}</h5>
        <p-table #table
                 [value]="data"
                 (onFilter)="onFilter()">
          >
          <ng-template pTemplate="header" let-columns>
            <tr>
              <th pSortableColumn="date" scope="col">
                Date
                <p-sortIcon field="date"/>
                <p-columnFilter type="date" field="date" display="menu"/>
              </th>
              <th pSortableColumn="temperatureC" scope="col">
                Temperature C
                <p-sortIcon field="temperatureC"/>
                <p-columnFilter type="numeric" field="temperatureC" display="menu"/>
              </th>
              <th pSortableColumn="temperatureF" scope="col">
                Temperature F
                <p-sortIcon field="temperatureF"/>
                <p-columnFilter type="numeric" field="temperatureF" display="menu"/>
              </th>
              <th pSortableColumn="summary" scope="col">
                Summary
                <p-sortIcon field="summary"/>
                <p-columnFilter type="text" field="summary" display="menu"/>
              </th>
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
    <div *ngIf="isSettings" class="flex flex-col md:flex-row gap-8">
      <div class="md:w-1/2">
        <div class="card flex flex-col gap-4">
          <div class="font-semibold text-xl">Weather settings</div>
          <div class="grid grid-cols-12 gap-4">
            <label for="dayCount" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Day Count</label>
            <div class="col-span-12 md:col-span-10">
              <input pInputText [(ngModel)]="settings.dayCount" id="dayCount" type="text"/>
            </div>
          </div>
          <div class="flex justify-end">
            <p-button label="Save" icon="pi pi-save" (onClick)="saveSettings(settings)"></p-button>
          </div>
        </div>
      </div>
    </div>
    <security *ngIf="isSecurity" [id]="id" [controller]="controller"></security>
  `,
  providers: [WeatherForecastModule],
  imports: [
    NgIf,
    TableModule,
    SharedModule,
    TagModule,
    SecurityComponent,
    Button,
    FormsModule,
    InputText,
  ],
})
export class WeatherForecastModuleComponent extends BaseModuleComponent<WeatherModuleSettings, WeatherModuleLocalSettings> implements OnInit, OnDestroy {
  @ViewChild('table') table!: Table;
  protected readonly dataService = inject(WeatherForecastModule);
  protected data: WeatherForecastResponse[] = [];

  constructor() {
    super();
  }

  async ngOnInit() {
    await super.ngOnInit();
    await this.dataService.weatherForecastModuleGetWeatherForecast({ dayCount: this.settings.dayCount }).then(result => {
      this.data = result;
    }, error => {
      console.error('Error fetching weather data:', error);
      this.msgService.error(error);
    });
    if (this.localSettings().filters)
      this.table.filters = this.localSettings().filters;
  }

  onFilter() {
    this.localSettings.update((settings) => ({ ...settings, filters: this.table.filters }));
  }
}
