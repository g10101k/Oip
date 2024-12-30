import { NgModule } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { WeatherComponent } from './weather.component';
import { RouterModule } from '@angular/router';
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TabViewModule } from "primeng/tabview";
import { WeatherSettingsComponent } from "./weather-settings/weather-settings.component";
import { WeatherSecurityComponent } from "./weather-security/weather-security.component";
import { WeatherDataService } from "./weather-data.service";


@NgModule({
  imports: [
    CommonModule,
    CurrencyPipe,
    DatePipe,
    TableModule,
    TagModule,
    TabViewModule,
    RouterModule.forChild([
      {
        path: '',
        component: WeatherComponent
      },
      {
        path: 'weather',
        component: WeatherComponent
      }
    ]),
  ],
  declarations: [
    WeatherComponent,
    WeatherSettingsComponent,
    WeatherSecurityComponent,
  ],
  providers: [WeatherDataService]
})
export class WeatherModule {
}
