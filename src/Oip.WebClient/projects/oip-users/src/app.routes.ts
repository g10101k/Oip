import { Routes } from '@angular/router';
import { AuthGuardService, NotfoundComponent, AppLayoutComponent } from 'oip-common';
import { inject } from '@angular/core';

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [() => inject(AuthGuardService).canActivate()],
    children: [
      {
        path: 'user/:id',
        loadComponent: () =>
          import('./app/components/users/users.component').then((m) => m.UserComponent),
        canActivate: [() => inject(AuthGuardService).canActivate()]
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
