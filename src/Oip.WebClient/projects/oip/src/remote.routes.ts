import { Routes } from '@angular/router';

export const remoteRoutes: Routes = [
  {
    path: 'dashboard/:id',
    loadComponent: () => import('./app/components/dashboard/dashboard.component').then((m) => m.DashboardComponent)
  },
  {
    path: 'weather-forecast-module/:id',
    loadComponent: () =>
      import('./app/components/weather-forecast-module/weather-forecast-module.component').then(
        (m) => m.WeatherForecastModuleComponent
      )
  },
  {
    path: 'customer-module/:id',
    loadComponent: () =>
      import('./app/components/customer-module/customer-module.component').then((m) => m.CustomerModuleComponent)
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard/0'
  }
];

