import { oipAuthGuard, provideOipRoutes } from 'oip-common';

export const appRoutes = provideOipRoutes({
  children: [
    {
      path: 'dashboard/:id',
      loadComponent: () => import('./app/components/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      canActivate: [oipAuthGuard]
    },
    {
      path: 'weather-forecast-module/:id',
      loadComponent: () =>
        import('./app/components/weather-forecast-module/weather-forecast-module.component').then(
          (m) => m.WeatherForecastModuleComponent
        ),
      canActivate: [oipAuthGuard]
    },
    {
      path: 'customer-module/:id',
      loadComponent: () =>
        import('./app/components/customer-module/customer-module.component').then((m) => m.CustomerModuleComponent),
      canActivate: [oipAuthGuard]
    }
  ]
});
