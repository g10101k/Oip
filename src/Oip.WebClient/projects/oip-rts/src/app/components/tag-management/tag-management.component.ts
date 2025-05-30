import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BaseComponent, Feature, SecurityComponent } from 'oip-common'
import { TagModule } from 'primeng/tag';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { NgIf } from '@angular/common';
import { TagManagementSettings, WeatherSettingsDto } from "./tag-management-settings.component";
import { TagManagementService } from "../../../api/TagManagement";
import { TagEntity } from "../../../api/data-contracts";

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
    <tag-management-settings *ngIf="isSettings"
                             [(settings)]="settings"
                             (settingsChange)="onSettingsChange($event)"></tag-management-settings>
    <security *ngIf="isSecurity" [id]="id" [controller]="controller"></security>
  `,
  providers: [],
  standalone: true,
  imports: [
    NgIf,
    TableModule,
    SharedModule,
    TagModule,
    TagManagementSettings,
    SecurityComponent,
    TagManagementSettings,
  ],
})
export class TagManagement extends BaseComponent<WeatherSettingsDto> implements OnInit, OnDestroy, Feature {
  controller: string = 'tag-management';
  servise = inject(TagManagementService);
  data: any;
  async ngOnInit() {
    await super.ngOnInit();

  }

  async onSettingsChange($event: WeatherSettingsDto) {
    await this.saveSettings($event)
  }
}
