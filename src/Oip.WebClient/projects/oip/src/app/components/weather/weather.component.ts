import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BaseComponent, Feature, SecurityComponent, SecurityStorageService } from 'oip-common'
import { WeatherForecast } from "../../api/WeatherForecast";
import { WeatherModuleSettings } from "../../api/data-contracts";
import { TagModule } from 'primeng/tag';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { NgIf } from '@angular/common';
import { Button } from "primeng/button";
import { FormsModule } from "@angular/forms";
import { InputText } from "primeng/inputtext";

@Component({
  selector: 'weather',
  template: `
    <div *ngIf="isContent" class="card">
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
            <p-button label="Save" icon="pi pi-save" (onClick)="saveButtonClick()"></p-button>
          </div>
        </div>
      </div>
    </div>
    <security *ngIf="isSecurity" [id]="id" [controller]="controller"></security>
  `,
  providers: [{ provide: 'controller', useValue: 'weather' }, WeatherForecast],
  standalone: true,
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
export class WeatherComponent extends BaseComponent<WeatherModuleSettings> implements OnInit, OnDestroy, Feature {
  controller: string = 'weather';
  protected readonly dataService = inject(WeatherForecast);
  protected data: any = [];

  constructor() {
    super();
  }

  async ngOnInit() {
    await super.ngOnInit();
    await this.dataService.weatherGetList({ dayCount: this.settings.dayCount }).then(result => {
      this.data = result.data;
    }, error => {
      this.msgService.error(error);
    });
  }

  async saveButtonClick() {
    await this.saveSettings(this.settings);
  }
}
