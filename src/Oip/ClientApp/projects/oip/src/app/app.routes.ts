import { Routes } from '@angular/router';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';
import { ModuleConfigComponent } from "./config/module-config.component";
import { AuthCallbackComponent } from "./auth-callback/auth-callback.component";
import { AuthGuardService } from "../auth/auth.service";

export const APP_ROUTES: Routes = [
  {
    path: '',
    loadChildren: () => import('./demo/components/dashboard/dashboard.module').then(m => m.DashboardModule),
    canActivate: [AuthGuardService]
  },
  {
    path: 'blocks',
    loadChildren: () => import('./demo/components/primeblocks/primeblocks.module').then(m => m.PrimeBlocksModule),
    canActivate: [AuthGuardService]
  },
  {
    path: 'uikit',
    loadChildren: () => import('./demo/components/uikit/uikit.module').then(m => m.UIkitModule),
    canActivate: [AuthGuardService]
  },
  {
    path: 'utilities',
    loadChildren: () => import('./demo/components/utilities/utilities.module').then(m => m.UtilitiesModule),
    canActivate: [AuthGuardService]
  },
  {
    path: 'documentation',
    loadChildren: () => import('./demo/components/documentation/documentation.module').then(m => m.DocumentationModule),
    canActivate: [AuthGuardService]
  },
  {
    path: 'auth-callback',
    component: AuthCallbackComponent,
  },
  {
    path: 'pages', loadChildren: () => import('./demo/components/pages/pages.module').then(m => m.PagesModule)
  },
  { path: 'auth', loadChildren: () => import('./demo/components/auth/auth.module').then(m => m.AuthModule) },
  {
    path: 'landing',
    loadChildren: () => import('./demo/components/landing/landing.module').then(m => m.LandingModule)
  },
  {
    path: 'config',
    component: ModuleConfigComponent,
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
