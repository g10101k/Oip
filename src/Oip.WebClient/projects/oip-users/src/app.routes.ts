import { Routes } from '@angular/router';
import { AuthGuardService, NotfoundComponent, AppLayoutComponent } from 'oip-common';
import { inject } from '@angular/core';

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)],
    children: [
      {
        path: 'user/:id',
        loadComponent: () => import('./app/components/users/users.component').then((m) => m.UserComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
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
