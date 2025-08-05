import { Routes } from '@angular/router';
import { NotfoundComponent, AppLayout, canActivateAuthRole } from "oip-common";

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayout,
    canActivate: [canActivateAuthRole],
    children: [
      {
        path: 'dashboard/:id',
        loadComponent: () => import('./app/components/dashboard/dashboard.component').then(m => m.DashboardComponent),
        canActivate: [canActivateAuthRole],
      },
      {
        path: 'weather-forecast-module/:id',
        loadComponent: () => import('./app/components/weather-forecast-module/weather-forecast-module.component').then(m => m.WeatherForecastModuleComponent),
        canActivate: [canActivateAuthRole]
      },
      {
        path: 'error',
        loadComponent: () => import('oip-common').then(m => m.ErrorComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('oip-common').then(m => m.ProfileComponent),
        canActivate: [canActivateAuthRole],
      },
      {
        path: 'config',
        loadComponent: () => import('oip-common').then(m => m.ConfigComponent),
        canActivate: [canActivateAuthRole],
      },
      {
        path: 'db-migration/:id',
        loadComponent: () => import('oip-common').then(m => m.DbMigrationComponent),
        canActivate: [canActivateAuthRole]
      },
      {
        path: 'modules',
        loadComponent: () => import('oip-common').then(m => m.AppModulesComponent),
        canActivate: [canActivateAuthRole]
      }
    ]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('oip-common').then(m => m.UnauthorizedComponent)
  },
  { path: 'notfound', component: NotfoundComponent },
  { path: '**', redirectTo: '/notfound' }
];
