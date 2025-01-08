import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

@NgModule({
  imports: [RouterModule.forChild([
    { path: 'error', loadComponent: () => import('./error/error.component').then(m => m.ErrorComponent) },
    { path: 'access', loadComponent: () => import('./access/access.component').then(m => m.AccessComponent) },
    { path: 'login', loadComponent: () => import('./login/login.component').then(m => m.LoginComponent) },
    { path: 'unauthorized', loadComponent: () => import('./unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent) },
    { path: '**', redirectTo: '/notfound' }
  ])],
  exports: [RouterModule]
})
export class AuthRoutingModule {
}
