import { Routes } from '@angular/router';
import {
  AuthGuardService,
  moduleAccessGuard,
  AccessComponent,
  NotfoundComponent,
  AppLayoutComponent
} from 'oip-common';
import { inject } from '@angular/core';

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)],
    canActivateChild: [moduleAccessGuard],
    children: [
      {
        path: 'tag-management/:id',
        loadComponent: () =>
          import('./app/components/tag-management/tag-management.component').then((m) => m.TagManagement),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'rtds-meta-data-context-migration-module/:id',
        loadComponent: () => import('oip-common').then((m) => m.DbMigrationComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)]
      },
      {
        path: 'access',
        component: AccessComponent
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
        path: 'applications',
        loadComponent: () => import('oip-common').then((m) => m.ApplicationsComponent),
        canActivate: [(_, state) => inject(AuthGuardService).canActivate(state.url)],
        data: { requireAdmin: true }
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
