import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { AuthGuardService } from 'oip-common';

export const remoteRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/sample-module/sample-module.component').then((m) => m.SampleModuleComponent),
    canActivate: [() => inject(AuthGuardService).canActivate()]
  }
];
