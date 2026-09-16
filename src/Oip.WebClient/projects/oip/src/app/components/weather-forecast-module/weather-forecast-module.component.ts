import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { BaseModuleComponent, SecurityComponent } from 'oip-common';
import { WeatherForecastModuleApi } from '../../../api/weather-forecast-module.api';
import { WeatherForecastResponse, WeatherModuleSettings } from '../../../api/data-contracts';
import { TagModule } from 'primeng/tag';
import { FilterMetadata, SharedModule } from 'primeng/api';
import { Table, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { Tooltip } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { DatePipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { provideTranslations } from 'oip-common';
import en from './l10n/weather-forecast-module.en.json';
import ru from './l10n/weather-forecast-module.ru.json';

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
      <div class="flex flex-col md:flex-row gap-4">
        <div class="card w-full">
          <div class="mb-4">
            <p-toolbar>
              <div class="flex flex-col md:flex-row md:items-center gap-2 w-full">
                <div class="font-semibold text-lg flex items-center gap-2 mx-2">
                  <i class="pi pi-cloud"></i>
                  {{ title }}
                </div>

                <div class="flex-1"></div>
                <div class="flex items-center gap-1 w-full md:w-auto">
                  <p-button
                    icon="pi pi-refresh"
                    rounded="true"
                    severity="secondary"
                    text="true"
                    tooltipPosition="bottom"
                    [loading]="loading"
                    [pTooltip]="'weather-forecast-module.content.refreshTooltip' | translate"
                    (onClick)="refreshAction()"></p-button>
                  <input
                    class="w-full md:w-96"
                    pInputText
                    type="text"
                    [placeholder]="'weather-forecast-module.content.filterPlaceholder' | translate"
                    [(ngModel)]="globalFilter"
                    (ngModelChange)="table.filterGlobal($event, 'contains')"/>
                  <p-button
                    icon="pi pi-filter-slash"
                    rounded="true"
                    severity="secondary"
                    text="true"
                    tooltipPosition="bottom"
                    [disabled]="!hasActiveFilters"
                    [pTooltip]="'weather-forecast-module.content.clearFilterTooltip' | translate"
                    (onClick)="clearFilter()"></p-button>
                </div>
              </div>
            </p-toolbar>
          </div>
          <p-table
            #table
            class="mt-4"
            dataKey="date"
            sortField="date"
            [sortOrder]="1"
            [paginator]="true"
            [rows]="20"
            [globalFilterFields]="['summary']"
            [value]="data"
            [loading]="loading"
            (onFilter)="onFilter()">
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="date" scope="col">
                  {{ 'weather-forecast-module.content.table.date' | translate }}
                  <p-sortIcon field="date" />
                  <p-columnFilter display="menu" field="date" type="date" />
                </th>
                <th pSortableColumn="temperatureC" scope="col">
                  {{ 'weather-forecast-module.content.table.temperatureC' | translate }}
                  <p-sortIcon field="temperatureC" />
                  <p-columnFilter display="menu" field="temperatureC" type="numeric" />
                </th>
                <th pSortableColumn="temperatureF" scope="col">
                  {{ 'weather-forecast-module.content.table.temperatureF' | translate }}
                  <p-sortIcon field="temperatureF" />
                  <p-columnFilter display="menu" field="temperatureF" type="numeric" />
                </th>
                <th pSortableColumn="summary" scope="col">
                  {{ 'weather-forecast-module.content.table.summary' | translate }}
                  <p-sortIcon field="summary" />
                  <p-columnFilter display="menu" field="summary" type="text" />
                </th>
              </tr>
            </ng-template>
            <ng-template let-forecast pTemplate="body">
              <tr>
                <td>{{ forecast.date | date: layoutService.dateTimeFormat() }}</td>
                <td>{{ forecast.temperatureC }}</td>
                <td>{{ forecast.temperatureF }}</td>
                <td>
                  <p-tag severity="success" [value]="forecast.summary"></p-tag>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="4">{{ 'weather-forecast-module.content.table.empty' | translate }}</td>
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
                <input id="dayCount" pInputText type="text" [(ngModel)]="settings.dayCount" />
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
      <security [controller]="controller" [id]="id" />
    }
  `,
  providers: [WeatherForecastModuleApi],
  imports: [
    TableModule,
    SharedModule,
    TagModule,
    SecurityComponent,
    ButtonModule,
    ToolbarModule,
    Tooltip,
    FormsModule,
    InputTextModule,
    DatePipe,
    TranslatePipe
  ]
})
export class WeatherForecastModuleComponent
  extends BaseModuleComponent<WeatherModuleSettings, WeatherModuleLocalSettings>
  implements OnInit, OnDestroy
{
  private readonly translations = provideTranslations({ en, ru });

  @ViewChild('table') table!: Table;
  protected readonly dataService = inject(WeatherForecastModuleApi);
  protected data: WeatherForecastResponse[] = [];
  protected loading = false;
  protected globalFilter = '';

  protected override async onModuleInstanceChange(): Promise<void> {
    await this.refreshAction();
    const filters = this.localSettings().filters;
    if (filters && this.table) {
      this.table.filters = filters;
      const global = filters['global'];
      this.globalFilter = (Array.isArray(global) ? global[0]?.value : global?.value) ?? '';
    }
  }

  async refreshAction() {
    this.loading = true;
    try {
      this.data = await this.dataService.getWeatherForecast({
        dayCount: this.settings.dayCount
      });
    } catch (error) {
      this.data = [];
      this.msgService.errorFromException(error, this.t('weather-forecast-module.errorFetchingMessage'));
    } finally {
      this.loading = false;
    }
  }

  protected get hasActiveFilters(): boolean {
    if (!this.table?.filters) return false;
    return Object.values(this.table.filters).some((meta) =>
      (Array.isArray(meta) ? meta : [meta]).some((m) => m?.value !== null && m?.value !== undefined && m?.value !== '')
    );
  }

  clearFilter() {
    this.globalFilter = '';
    this.table.clearFilterValues();
    this.table._filter();
  }

  onFilter() {
    this.localSettings.update((settings) => ({
      ...settings,
      filters: this.table.filters
    }));
  }
}
