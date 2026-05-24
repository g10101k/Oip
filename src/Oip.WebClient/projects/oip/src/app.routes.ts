import { Routes } from '@angular/router';
import { AuthGuardService, NotfoundComponent, AppLayoutComponent, DiscussionComponent } from 'oip-common';
import { inject } from '@angular/core';

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)],
    children: [
      {
        path: 'dashboard/:id',
        loadComponent: () => import('./app/components/dashboard/dashboard.component').then((m) => m.DashboardComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'weather-forecast-module/:id',
        loadComponent: () =>
          import('./app/components/weather-forecast-module/weather-forecast-module.component').then(
            (m) => m.WeatherForecastModuleComponent
          ),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'customer-module/:id',
        loadComponent: () =>
          import('./app/components/customer-module/customer-module.component').then(
            (m) => m.CustomerModuleComponent
          ),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'discussion/:id',
        loadComponent: () => import('oip-common').then((m) => m.DiscussionComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'error',
        loadComponent: () => import('oip-common').then((m) => m.ErrorComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('oip-common').then((m) => m.ProfileComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'config',
        loadComponent: () => import('oip-common').then((m) => m.ConfigComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'db-migration/:id',
        loadComponent: () => import('oip-common').then((m) => m.DbMigrationComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'modules',
        loadComponent: () => import('oip-common').then((m) => m.AppModulesComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'applications',
        loadComponent: () => import('oip-common').then((m) => m.ApplicationsComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'iframe-module/:id',
        loadComponent: () => import('oip-common').then((m) => m.IframeModuleComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      }
    ]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('oip-common').then((m) => m.UnauthorizedComponent)
  },
  {path: 'notfound', component: NotfoundComponent},
  {path: '**', redirectTo: '/notfound'}
];
