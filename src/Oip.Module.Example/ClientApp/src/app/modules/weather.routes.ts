import { Routes } from '@angular/router';
import { WeatherComponent } from './weather/weather.component';

export const FLIGHTS_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'weather',
    pathMatch: 'full'
  },
  {
    path: 'weather',
    component: WeatherComponent
  }
];
