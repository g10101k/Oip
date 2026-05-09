import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { AppLayoutComponent, AuthGuardService, NotfoundComponent } from 'oip-common';
import { remoteRoutes } from './remote.routes';

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [() => inject(AuthGuardService).canActivate()],
    children: [
      {
        path: 'sample-module/:id',
        children: remoteRoutes
      },
      {
        path: 'profile',
        loadComponent: () => import('oip-common').then((m) => m.ProfileComponent),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      },
      {
        path: 'config',
        loadComponent: () => import('oip-common').then((m) => m.ConfigComponent),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      }
    ]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('oip-common').then((m) => m.UnauthorizedComponent)
  },
  { path: 'notfound', component: NotfoundComponent },
  { path: '**', redirectTo: '/notfound' }
];
