import { Routes } from '@angular/router';
import { AuthGuardService, NotfoundComponent, AppLayoutComponent, DiscussionComponent } from 'oip-common';
import { inject } from '@angular/core';

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [() => inject(AuthGuardService).canActivate()],
    children: [
      {
        path: 'external-module-example-module/:id',
        loadComponent: () =>
          import('./app/components/external-module-example-module/external-module-example-module.component').then(
            (m) => m.ExternalModuleExampleModuleComponent
          ),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      },
      {
        path: 'discussion/:id',
        loadComponent: () => import('oip-common').then((m) => m.DiscussionComponent),
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
      },
      {
        path: 'db-migration/:id',
        loadComponent: () => import('oip-common').then((m) => m.DbMigrationComponent),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      },
      {
        path: 'modules',
        loadComponent: () => import('oip-common').then((m) => m.AppModulesComponent),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      },
      {
        path: 'iframe-module/:id',
        loadComponent: () => import('oip-common').then((m) => m.IframeModuleComponent),
        canActivate: [() => inject(AuthGuardService).canActivate()]
      },

    ]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('oip-common').then((m) => m.UnauthorizedComponent)
  },
  {path: 'notfound', component: NotfoundComponent},
  {path: '**', redirectTo: '/notfound'}
];
