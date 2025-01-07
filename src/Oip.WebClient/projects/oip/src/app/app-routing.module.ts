import { RouterModule, UrlSegment } from '@angular/router';
import { inject, NgModule } from '@angular/core';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';
import { AuthGuardService, AppLayoutComponent } from "oip/common";

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
            loadChildren: () => import('oip/common').then(m => m.AppConfigModule),
            canActivate: [() => inject(AuthGuardService).canActivate()]
          },
          {
            path: 'weather/:id',
            loadChildren: () => import('./demo/components/weather/weather.module').then(m => m.WeatherModule),
            canActivate: [() => inject(AuthGuardService).canActivate()]
          },

          {
            path: 'error',
            loadChildren: () => import('./demo/components/auth/error/error.module').then(m => m.ErrorModule)
          }
        ]
      },
      {
        path: 'unauthorized',
        loadChildren: () => import('./demo/components/auth/unauthorized/unauthorized.module').then(m => m.UnauthorizedModule)
      },
      {
        path: 'blocks',
        loadChildren: () => import('./demo/components/primeblocks/primeblocks.module').then(m => m.PrimeBlocksModule)
      },
      { path: 'auth', loadChildren: () => import('./demo/components/auth/auth.module').then(m => m.AuthModule) },
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
