import { NgModule } from '@angular/core';
import { Route, RouterModule } from '@angular/router';


export const AuthRouting: Route[] = [

  { path: 'error', loadChildren: () => import('./error/error.module').then(m => m.ErrorModule) },
  { path: 'access', loadChildren: () => import('./access/access.component').then(m => m.AccessComponent) },
  { path: 'login', loadChildren: () => import('./login/login.module').then(m => m.LoginModule) },
  { path: 'unauthorized', loadChildren: () => import('./unauthorized/unauthorized.module').then(m => m.UnauthorizedModule) },
  { path: '**', redirectTo: '/notfound' }
];
