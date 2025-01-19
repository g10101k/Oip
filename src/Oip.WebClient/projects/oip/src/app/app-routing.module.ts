import { RouterModule } from '@angular/router';
import { inject, NgModule } from '@angular/core';
import { AuthGuardService, AppLayoutComponent, NotfoundComponent } from "oip-common";

@NgModule({
  imports: [
    RouterModule.forRoot([
      {
        path: '', component: AppLayoutComponent,
        canActivate: [() => inject(AuthGuardService).canActivate()],
        children: [
          {
            path: 'dashboard/:id',
            loadChildren: () => import('./demo/components/dashboard/dashboard.module').then(m => m.DashboardModule),
            canActivate: [() => inject(AuthGuardService).canActivate()],
          },
          {
            path: 'uikit',
            loadChildren: () => import('./demo/components/uikit/uikit.module').then(m => m.UIkitModule),
            canActivate: [() => inject(AuthGuardService).canActivate()],
          },
          {
            path: 'utilities',
            loadChildren: () => import('./demo/components/utilities/utilities.module').then(m => m.UtilitiesModule),
            canActivate: [() => inject(AuthGuardService).canActivate()],
          },
          {
            path: 'documentation',
            loadChildren: () => import('./demo/components/documentation/documentation.module').then(m => m.DocumentationModule),
            canActivate: [() => inject(AuthGuardService).canActivate()],
          },
          {
            path: 'pages',
            loadChildren: () => import('./demo/components/pages/pages.module').then(m => m.PagesModule),
            canActivate: [() => inject(AuthGuardService).canActivate()],
          },
          {
            path: 'config',
            loadComponent: () => import('oip-common').then(m => m.ConfigComponent),
            canActivate: [() => inject(AuthGuardService).canActivate()]
          },
          {
            path: 'weather/:id',
            loadChildren: () => import('./demo/components/weather/weather.module').then(m => m.WeatherModule),
            canActivate: [() => inject(AuthGuardService).canActivate()]
          },
          {
            path: 'error',
            loadComponent: () => import('oip-common').then(m => m.ErrorComponent)
          },
          {
            path: 'profile',
            loadComponent: () => import('oip-common').then(m => m.ProfileComponent),
            canActivate: [() => inject(AuthGuardService).canActivate()],
          }
        ]
      },
      {
        path: 'unauthorized',
        loadComponent: () => import('oip-common').then(m => m.UnauthorizedComponent)
      },
      {
        path: 'blocks',
        loadChildren: () => import('./demo/components/primeblocks/primeblocks.module').then(m => m.PrimeBlocksModule)
      },
      { path: 'auth', loadChildren: () => import('oip-common').then(m => m.AuthModule) },
      {
        path: 'landing',
        loadChildren: () => import('./demo/components/landing/landing.module').then(m => m.LandingModule)
      },
      { path: 'notfound', component: NotfoundComponent },
      { path: '**', redirectTo: '/notfound' },
    ], { scrollPositionRestoration: 'enabled', anchorScrolling: 'enabled', onSameUrlNavigation: 'reload' })
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
