import { NgModule } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { WeatherComponent } from './weather.component';
import { RouterModule } from '@angular/router';
import { AuthLibModule } from 'auth-lib';
import { SharedLibModule } from 'shared-lib';
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TabViewModule } from "primeng/tabview";
import { WeatherSettingsComponent } from "./weather-settings/weather-settings.component";
import { WeatherSecurityComponent } from "./weather-security/weather-security.component";


@NgModule({
    imports: [
        CommonModule,
        AuthLibModule,
        SharedLibModule,
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
    exports: [
        WeatherSecurityComponent
    ],
    declarations: [
        WeatherComponent,
        WeatherSettingsComponent,
        WeatherSecurityComponent,
    ]
})
export class WeatherModule {
}
