import { Routes } from '@angular/router';
import { AuthGuardService, NotfoundComponent, AppLayout } from 'oip-common';
import { inject } from '@angular/core';

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayout,
    canActivate: [() => inject(AuthGuardService).canActivate()],
    children: [
      {
        path: 'tag-management/:id',
        loadComponent: () =>
          import('./app/components/tag-management/tag-management.component').then((m) => m.TagManagement),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      },
      {
        path: 'rtds-meta-data-context-migration-module/:id',
        loadComponent: () => import('oip-common').then((m) => m.DbMigrationComponent),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      },
      {
        path: 'error',
        loadComponent: () => import('oip-common').then((m) => m.ErrorComponent)
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
