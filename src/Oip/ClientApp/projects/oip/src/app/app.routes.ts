import { Routes } from '@angular/router';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';
import { AuthGuardService } from "./services/auth.service";
import { inject } from "@angular/core";

export const APP_ROUTES: Routes = [
  {
    path: '',
    loadChildren: () => import('./demo/components/dashboard/dashboard.module').then(m => m.DashboardModule),
    canActivate: [() => inject(AuthGuardService).canActivate()]
  },
  {
    path: 'blocks',
    loadChildren: () => import('./demo/components/primeblocks/primeblocks.module').then(m => m.PrimeBlocksModule),
    canActivate: [() => inject(AuthGuardService).canActivate()]
  },
  {
    path: 'uikit',
    loadChildren: () => import('./demo/components/uikit/uikit.module').then(m => m.UIkitModule),
    canActivate: [() => inject(AuthGuardService).canActivate()]
  },
  {
    path: 'utilities',
    loadChildren: () => import('./demo/components/utilities/utilities.module').then(m => m.UtilitiesModule),
    canActivate: [() => inject(AuthGuardService).canActivate()]
  },
  {
    path: 'documentation',
    loadChildren: () => import('./demo/components/documentation/documentation.module').then(m => m.DocumentationModule),
    canActivate: [() => inject(AuthGuardService).canActivate()]
  },
  {
    path: 'pages', loadChildren: () => import('./demo/components/pages/pages.module').then(m => m.PagesModule)
  },
  {
    path: 'auth', loadChildren: () => import('./demo/components/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'landing',
    loadChildren: () => import('./demo/components/landing/landing.module').then(m => m.LandingModule)
  },
  {
    path: 'config',
    loadChildren: () => import('./layout/config/config.module').then(m => m.AppConfigModule),
    pathMatch: 'full',
  },
  {
    path: 'unauthorized',
    loadChildren: () => import('./demo/components/auth/error/error.module').then(m => m.ErrorModule)
  }
];

export const APP_ROUTES_END: Routes = [
  {
    path: '**',
    component: NotfoundComponent,
  },
]
